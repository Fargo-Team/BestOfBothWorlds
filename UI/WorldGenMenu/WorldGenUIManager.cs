using FargoSeeds.Content;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
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

    private static UIElement? container;

    private static UIElement? infoPageContainer;

    private static UIElement? infoPage;

    private static UIElement? moddedInfoPage;

    private static UIWorldGenTab? vanillaTab;

    private static UIWorldGenTab? moddedTab;

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
                container = element;
            }
        );
    }

    private static void MakeInfoMenu_CacheInfoPage_AddTabs(On_UIWorldCreation.orig_MakeInfoMenu orig, UIWorldCreation self, UIElement parentContainer)
    {
        orig(self, parentContainer);

        creationUi = self;

        infoPageContainer = parentContainer;
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

        return;

        static void SideTabs_OnUpdate_UpdatePosition(UIElement element)
        {
            if (container is null)
            {
                return;
            }

            var contDims = container.Dimensions;

            element.Left.Set(contDims.Right + 10, 0f);
            element.Top.Set(contDims.Y, 0f);

            element.Height.Set(contDims.Height, 0f);

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

        infoPageContainer.RemoveChild(moddedInfoPage);

        if (!infoPageContainer.HasChild(infoPage))
        {
            infoPageContainer.Append(infoPage);
        }
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

        infoPageContainer.RemoveChild(infoPage);

        if (!infoPageContainer.HasChild(moddedInfoPage))
        {
            BuildModdedInfoPage();

            infoPageContainer.Append(moddedInfoPage);
        }

        return;

        void BuildModdedInfoPage()
        {
            moddedInfoPage = new UIElement();
            {
                moddedInfoPage.Width.Set(0f, 1f);
                moddedInfoPage.Height.Set(0f, 1f);
                moddedInfoPage.HAlign = 0.5f;

                moddedInfoPage.SetPadding(10f);
                moddedInfoPage.PaddingBottom = 0f;
                moddedInfoPage.PaddingTop = 0f;
            }

            creationUi?.AddDescriptionPanel(moddedInfoPage, 0f, string.Empty);

            var above = new UIElement();
            {
                above.Width.Set(0f, 1f);
                above.Height.Set(-48, 1f);
            }
            moddedInfoPage.Append(above);

            var scrollbar = new UIScrollbar().WithView(100f, 1000f);
            {
                scrollbar.HAlign = 1f;
                scrollbar.Height.Set(0f, 1f);
            }
            above.Append(scrollbar);

            var list = new UIList();
            {
                list.SetScrollbar(scrollbar);

                list.Height.Set(0f, 1f);
                list.Width.Set(-(scrollbar.Width.Pixels + 4f), 1f);
            }
            above.Append(list);

            AddOptions(list);

            list.Activate();

            moddedInfoPage.Recalculate();
        }
    }

    private static void AddOptions(UIList list)
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
            }
            header.Append(text);

            var div = new UIHorizontalSeparator();
            {
                div.Width.Set(0f, 1f);
                div.VAlign = 1f;
                div.Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f;
            }
            header.Append(div);

            var sorted = WorldGenManager.OPTIONS_BY_CATEGORY[category].OrderBy(o => o.Priority).ToList();

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
                }
                container.Append(left);

                if (i + 1 < sorted.Count)
                {
                    var right = sorted[i + 1].ProvideElement();
                    {
                        right.Height.Set(34f, 0f);
                        right.Width.Set(-3f, 0.5f);

                        right.HAlign = 1f;
                    }
                    container.Append(right);
                }
            }
        }
    }
}