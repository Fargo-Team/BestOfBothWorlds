using System;
using FargoSeeds.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public sealed class WorldGenUIManager : ILoadable
{
    void ILoadable.Load(Mod _)
    {
        IL_UIWorldCreation.BuildPage += BuildPage_CacheContainer;
        On_UIWorldCreation.MakeInfoMenu += MakeInfoMenu_CacheInfoPage_AddTabs;
        On_UIWorldCreation.SetupGamepadPoints += SetupGamepadPoints_TEMP_SkipGamepadPoints;
    }

    void ILoadable.Unload()
    { }

    private static UIWorldCreation? creationUi;

    private static UIElement? entireContainer;

    private static UIElement? infoPageContainer;

    private static UIElement? infoPage;

    private static UIElement? moddedInfoPage;

    private static UIWorldGenTab? vanillaTab;

    private static UIWorldGenTab? moddedTab;

    private static UIText? modDescText;

    private static UIList? optionsList;

    private static float vanillaPanelHeight;

    private static float vanillaEntireHeight;

    private static UIElement? presetContainer;

    private void SetupGamepadPoints_TEMP_SkipGamepadPoints(On_UIWorldCreation.orig_SetupGamepadPoints orig, UIWorldCreation self, SpriteBatch sb)
    {
        if (infoPageContainer?.HasChild(infoPage) is not true)
        {
            return;
        }

        orig(self, sb);
    }

    private static void BuildPage_CacheContainer(ILContext il)
    {
        var c = new ILCursor(il);

        var uiElementIndex = -1;

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdloc(out uiElementIndex),
            i => i.MatchCall<UIElement>(nameof(UIElement.Append))
        );

        c.EmitLdloc(uiElementIndex);

        c.EmitDelegate(
            static (UIElement element) =>
            {
                entireContainer = element;
            }
        );
    }

    private static void MakeInfoMenu_CacheInfoPage_AddTabs(On_UIWorldCreation.orig_MakeInfoMenu orig, UIWorldCreation self, UIElement parentContainer)
    {
        orig(self, parentContainer);

        creationUi = self;

        infoPageContainer = parentContainer;

        creationUi.Recalculate();

        vanillaEntireHeight = entireContainer!.Dimensions.Height;
        vanillaPanelHeight = infoPageContainer.Dimensions.Height;

        infoPage = parentContainer.Children.First();

        var sideTabs = new UIElement();
        {
            sideTabs.SetPadding(0f);

            sideTabs.Width.Set(44f, 0f);

            // TODO: Maybe recalculate is better? idk im tired boss
            sideTabs.OnUpdate += SideTabs_OnUpdate_UpdatePosition;
        }
        self.Append(sideTabs);

        // TODO: (if multiple tabs are needed, use a VAlign = 0.5 UIList)

        vanillaTab = new UIWorldGenTab(
            Mods.FargoSeeds.WorldGenMenu.StandardSetting.GetText(),
            Assets.Images.UI.UIWorldGenTab_Vanilla.Asset
        );
        {
            vanillaTab.Enabled = true;
            vanillaTab.VAlign = 0.5f;
            vanillaTab.Top.Set(-(22f + 4f), 0f);

            vanillaTab.OnLeftClick += OpenVanillaUI;
        }
        sideTabs.Append(vanillaTab);

        moddedTab = new UIWorldGenTab(
            Mods.FargoSeeds.WorldGenMenu.ModSetting.GetText(),
            Assets.Images.UI.UIWorldGenTab_Modded.Asset
        );
        {
            moddedTab.VAlign = 0.5f;
            moddedTab.Top.Set((22f + 4f), 0f);

            moddedTab.OnLeftClick += OpenModdedUI;
        }
        sideTabs.Append(moddedTab);

        presetContainer = new UIElement();
        {
            presetContainer.Width.Set(300f, 0f);

            presetContainer.OnUpdate += PresetContainerOnUpdateUpdatePosition;
        }

        AddPresets(presetContainer);

        return;

        static void PresetContainerOnUpdateUpdatePosition(UIElement element)
        {
            if (infoPageContainer is null)
            {
                return;
            }

            var contDims = infoPageContainer.Dimensions;

            element.Left.Set(contDims.Left - 10 - element.Width.Pixels, 0f);
            element.Top.Set(contDims.Y, 0f);

            element.Height.Set(contDims.Height, 0f);

            element.Recalculate();
        }

        static void SideTabs_OnUpdate_UpdatePosition(UIElement element)
        {
            if (infoPageContainer is null)
            {
                return;
            }

            var contDims = infoPageContainer.Dimensions;

            element.Left.Set(contDims.Right + 10, 0f);
            element.Top.Set(contDims.Y, 0f);

            element.Height.Set(vanillaPanelHeight, 0f);

            element.Recalculate();
        }
    }

    private static void OpenVanillaUI(UIMouseEvent evt, UIElement element)
    {
        if (infoPage is null
         || infoPageContainer is null
         || element is not UIWorldGenTab tab
         || tab.Enabled)
        {
            return;
        }

        tab.Enabled = true;

        moddedTab?.Enabled = false;

        infoPageContainer.Parent.Height.Set(vanillaPanelHeight, 0f);
        entireContainer!.Height.Set(vanillaEntireHeight, 0f);

        infoPageContainer.RemoveChild(moddedInfoPage);

        if (!infoPageContainer.HasChild(infoPage))
        {
            infoPageContainer.Append(infoPage);
        }

        creationUi!.RemoveChild(presetContainer);

        entireContainer.Recalculate();
    }

    private static void OpenModdedUI(UIMouseEvent evt, UIElement element)
    {
        if (infoPage is null
         || infoPageContainer is null
         || element is not UIWorldGenTab tab
         || tab.Enabled)
        {
            return;
        }

        tab.Enabled = true;

        vanillaTab?.Enabled = false;

        const float height = 450;

        infoPageContainer.Parent.Height.Set(height + 18f, 0f);
        entireContainer!.Height.Set(vanillaEntireHeight + ((height + 18f) - vanillaPanelHeight), 0f);

        infoPageContainer.RemoveChild(infoPage);

        if (!infoPageContainer.HasChild(moddedInfoPage))
        {
            BuildModdedInfoPage();

            infoPageContainer.Append(moddedInfoPage);
        }

        creationUi!.Append(presetContainer);

        entireContainer.Recalculate();

        return;

        void BuildModdedInfoPage()
        {
            const float description_height = 50;

            moddedInfoPage = new UIElement();
            {
                moddedInfoPage.Width.Set(0f, 1f);
                moddedInfoPage.Height.Set(0f, 1f);
                moddedInfoPage.HAlign = 0.5f;

                moddedInfoPage.SetPadding(10f);
                moddedInfoPage.PaddingBottom = 0f;
                moddedInfoPage.PaddingTop = 3f;
            }

            AddDescriptionPanel(moddedInfoPage, description_height);

            const float above_size = 40f;

            var above = new UIElement();
            {
                above.Width.Set(0f, 1f);
                above.Height.Set(above_size, 0);
            }
            moddedInfoPage.Append(above);

            var searchBar = new UISearchBar("...");
            {
                searchBar.Width.Set(0f, 0.5f);
                searchBar.HAlign = 1f;
                searchBar.VAlign = 0f;

                searchBar.Input.OnTextChanged += OnTextChanged_UpdateSearch;
            }
            above.Append(searchBar);

            var centerContainer = new UIElement();
            {
                centerContainer.Width.Set(0f, 1f);
                centerContainer.Height.Set(-(description_height + 8f + above_size), 1f);
                centerContainer.Top.Set(above_size, 0f);
            }
            moddedInfoPage.Append(centerContainer);

            var scrollbar = new UIScrollbar().WithView(100f, 1000f);
            {
                scrollbar.HAlign = 1f;
                scrollbar.Height.Set(0f, 1f);
            }
            centerContainer.Append(scrollbar);

            optionsList = new UIList();
            {
                optionsList.SetScrollbar(scrollbar);

                optionsList.Height.Set(0f, 1f);
                optionsList.Width.Set(-(scrollbar.Width.Pixels + 4f), 1f);

                optionsList.ManualSortMethod = _ => { };
            }
            centerContainer.Append(optionsList);

            AddOptions(optionsList);

            optionsList.Activate();

            moddedInfoPage.Recalculate();
        }

        static void AddDescriptionPanel(UIElement parent, float height)
        {
            var panel = new UISlicedImage(Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelHighlight"));
            {
                panel.HAlign = 0.5f;
                panel.VAlign = 1f;
                panel.Width.Set(0f, 1f);
                panel.Left.Set(0f, 0f);
                panel.Height.Set(height, 0f);
                panel.Top.Set(2f, 0f);

                panel.SetSliceDepths(10);
                panel.Color = Color.LightGray * 0.7f;
            }
            parent.Append(panel);

            modDescText = new UIText(Language.GetText("UI.WorldDescriptionDefault"), 0.82f);
            {
                modDescText.HAlign = 0f;
                modDescText.VAlign = 0.5f;
                modDescText.Width.Set(0f, 1f);
                modDescText.Height.Set(0f, 1f);

                modDescText.PaddingTop = 6f;
                modDescText.PaddingLeft = 4f;
                modDescText.PaddingRight = 4f;

                modDescText.TextOriginY = 0.5f;

                modDescText.WrappedTextBottomPadding = 0f;
                modDescText.IsWrapped = true;
            }
            panel.Append(modDescText);
        }
    }

    private static void OnTextChanged_UpdateSearch(UIInputField obj)
    {
        optionsList!.Clear();
        AddOptions(optionsList, obj.Text);
        optionsList.Activate();
    }

    private static void AddOptions(UIList list, string query = "")
    {
        var categories = WorldGenManager.OPTIONS_BY_CATEGORY.Keys.ToList().OrderBy(c => c.Priority);

        // TODO: Search logic
        foreach (var category in categories)
        {
            var header = new UIElement();
            {
                header.Width.Set(0f, 1f);
                header.Height.Set(30f, 0f);
            }
            list.Add(header);

            var text = new UIText(Language.GetText(category.Name));
            {
                text.Width.Set(0f, 1f);
                text.Height.Set(0f, 1f);

                text.VAlign = 0.5f;
                text.HAlign = 0.5f;

                text.IgnoresMouseInteraction = true;
            }
            header.Append(text);

            var div = new UIHorizontalSeparator();
            {
                div.Width.Set(0f, 1f);
                div.VAlign = 1f;
                div.Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f;
            }
            header.Append(div);

            Assets.Images.UI.Reset.Asset.Wait();
            var resetButton = new UIImageButton(Assets.Images.UI.Reset.Asset);
            {
                resetButton.VAlign = 0.5f;

                resetButton.Left.Set(4f, 0f);

                var inner = category;

                resetButton.OnLeftClick += (_, _) =>
                {
                    SoundEngine.PlaySound(in SoundID.MenuOpen);
                    WorldGenManager.ResetCategory(inner);
                };
            }
            header.Append(resetButton);

            var sorted = WorldGenManager.OPTIONS_BY_CATEGORY[category].OrderBy(o => o.Priority).ToList();

            // TODO: Fuzzy matching
            if (!string.IsNullOrEmpty(query))
            {
                sorted.RemoveAll(o => !Language.GetTextValue(o.Name).Contains(query, StringComparison.CurrentCultureIgnoreCase));
            }

            for (var i = 0; i < sorted.Count; i += 2)
            {
                var container = new UIElement();
                {
                    container.Height.Set(34f, 0f);
                    container.Width.Set(0f, 1f);
                }
                list.Add(container);

                var left = sorted[i].ProvideElement();
                {
                    left.Height.Set(34f, 0f);
                    left.Width.Set(-3f, 0.5f);

                    left.OnMouseOver += ShowOptionDescription;
                    left.OnMouseOut += ClearOptionDescription;
                }
                container.Append(left);

                if (i + 1 < sorted.Count)
                {
                    var right = sorted[i + 1].ProvideElement();
                    {
                        right.Height.Set(34f, 0f);
                        right.Width.Set(-3f, 0.5f);

                        right.OnMouseOver += ShowOptionDescription;
                        right.OnMouseOut += ClearOptionDescription;

                        right.HAlign = 1f;
                    }
                    container.Append(right);
                }
            }
        }
    }

    private static void AddPresets(UIElement container)
    {
        var panel = new UIPanel();
        {
            panel.Width.Set(0f, 1f);
            panel.Height.Set(0f, 1f);

            panel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
        }
        container.Append(panel);


    }

    private static void ShowOptionDescription(UIMouseEvent evt, UIElement element)
    {
        if (element is not UIOptionElement opt)
        {
            return;
        }

        modDescText!.SetText(opt.Description);
    }

    private static void ClearOptionDescription(UIMouseEvent evt, UIElement element)
    {
        modDescText!.SetText(Language.GetText("UI.WorldDescriptionDefault"));
    }
}