using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader;
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
        }
    }
}