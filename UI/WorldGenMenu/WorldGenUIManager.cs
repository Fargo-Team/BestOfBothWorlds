using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace FargoSeeds.UI.WorldGenMenu;

internal class WorldGenUIManager : ModSystem
{
    public static bool ModdedMenuActive = false;
    public static bool HasClickedMenu = false;
    public static UIText DescriptionText = null;

    public UIElement InfoMenuParent;

    public static Dictionary<UIWorldGenCategory, List<UIElement>> ToggleCategories = [];

    public static int ToggleWidth = 222;
    public static int ToggleHeight = 34;

    public static void AddToggle(Mod mod, LocalizedText category, LocalizedText title, LocalizedText description, Color textColor, string iconTexturePath,  bool defaultValue, Action<bool> action, bool emode = false, float priority = 1f)
    {
        if (Main.dedServ)
            return;
            
        var uiToggle = new UIWorldGenToggle(mod, title, description, textColor, iconTexturePath, defaultValue, action, emode, priority)
        {
            Width = StyleDimension.FromPixels(ToggleWidth),
            Height = StyleDimension.FromPixels(ToggleHeight),
        };
        AddCategory(uiToggle, category);
    }
    public static void AddSlider(Mod mod, LocalizedText category, LocalizedText title, LocalizedText description, Color textColor, string iconTexturePath,  float defaultValue, Action<float> action, bool intSlider, List<float> sliderRange)
    {
        if (Main.dedServ)
            return;

        var uiToggle = new UIWorldGenSlider(mod, title, description, textColor, iconTexturePath, defaultValue, action, intSlider, sliderRange)
        {
            Width = StyleDimension.FromPixels(ToggleWidth),
            Height = StyleDimension.FromPixels(ToggleHeight),
        };
        AddCategory(uiToggle, category);
    }
    public static void AddCategory(UIElement element, LocalizedText categoryText)
    {
        if (Main.dedServ)
            return;

        UIWorldGenCategory category = ToggleCategories.Keys.FirstOrDefault(c => c.Text.Value == categoryText.Value, null);
        if (category == null)
        {
            category = new UIWorldGenCategory(categoryText, ToggleCategories.Count)
            {
                Width = StyleDimension.FromPixels(ToggleWidth),
                Height = StyleDimension.FromPixels(ToggleHeight),
            };
            ToggleCategories[category] = [];
        }
        ToggleCategories[category].Add(element);
    }
    /// <summary>
    /// Ascending order <br></br>
    /// By default, the priority of category number n (vertically) is n <br></br>
    /// Creates the category if it does not already exist
    /// </summary>
    public static void ChangeCategoryPriority(LocalizedText categoryText, float newPriority)
    {
        UIWorldGenCategory category = ToggleCategories.Keys.FirstOrDefault(c => c.Text.Value == categoryText.Value, null);
        if (category == null)
        {
            category = new UIWorldGenCategory(categoryText, ToggleCategories.Count)
            {
                Width = StyleDimension.FromPixels(ToggleWidth),
                Height = StyleDimension.FromPixels(ToggleHeight),
            };
            ToggleCategories[category] = [];
        }
        category.Priority = newPriority;
    }
    public static int TogglesPerRow => 2;
    public override void Load()
    {
        base.Load();
    }
    public override void PostSetupContent()
    {
        // load stuff
        On_UIWorldCreation.MakeInfoMenu += MakeInfoMenu_Detour;
        On_UIWorldCreation.FinishCreatingWorld += FinishCreatingWorld_Detour;
        On_UIWorldCreation.Click_NamingAndCreating += Click_NamingAndCreating_Detour;
        On_UIWorldCreation.MakeBackAndCreatebuttons += MakeBackAndCreatebuttons_Detour;
        On_UIWorldCreation.Click_GoBack += Click_GoBack_Detour;

        On_UIElement.Draw += UIElement_Draw_Detour;
        On_UIElement.Update += UIElement_Update_Detour;
        On_UIElement.ContainsPoint += UIElement_ContainsPoint_Detour;
    }

    UIPanel TogglePanel;

    UITextPanel<LocalizedText> CreateButton;

    private void MakeInfoMenu_Detour(On_UIWorldCreation.orig_MakeInfoMenu orig, UIWorldCreation self, UIElement parentContainer)
    {
        orig(self, parentContainer);

        HasClickedMenu = false; // This method runs every time the New World button is clicked; so we can put this here
        ModdedMenuActive = false;

        InfoMenuParent = parentContainer;
        int infoMenuHalfWidth = 278;
        int infoMenuHalfHeight = 225;

        // tab buttons
        // TODO: these aren't aligned correctly vertically if resolution is not 1920x1080
        var vanillaTab = new UIWorldGenTab(Language.GetText("Mods.FargoSeeds.WorldGenMenu.StandardSetting"), "FargoSeeds/UI/WorldGenMenu/UIWorldGenTab_Vanilla", () => !ModdedMenuActive, ToggleMenu)
        {
            HAlign = 0.5f,
            VAlign = 0.5f,
            Left = StyleDimension.FromPixels(infoMenuHalfWidth),
            Top = StyleDimension.FromPixels(-infoMenuHalfHeight + 0f)
        };
        var moddedTab = new UIWorldGenTab(Language.GetText("Mods.FargoSeeds.WorldGenMenu.ModSetting"), "FargoSeeds/UI/WorldGenMenu/UIWorldGenTab_Modded", () => ModdedMenuActive, ToggleMenu)
        {
            HAlign = 0.5f,
            VAlign = 0.5f,
            Left = StyleDimension.FromPixels(infoMenuHalfWidth),
            Top = StyleDimension.FromPixels(-infoMenuHalfHeight + 44f + 8f)
        };
        self.Append(vanillaTab);
        self.Append(moddedTab);

        // toggle panel
        /*

        float panelWidth = 198f * ToggleXAmt;
        TogglePanel = new()
        {
            Width = StyleDimension.FromPixels(panelWidth),
            Height = StyleDimension.FromPixels(260),
            HAlign = 0.5f,
            VAlign = 1,
            Top = StyleDimension.FromPixels(-32f),
            BackgroundColor = new Color(33, 43, 79) * 0.8f
        };
        */
        int num = 18;
        TogglePanel = new()
        {
            Width = StyleDimension.FromPercent(1f),
            Height = StyleDimension.FromPixels(280 + num),
            Top = StyleDimension.FromPixels(50f),
            BackgroundColor = new Color(33, 43, 79) * 0.8f
        };
        TogglePanel.SetPadding(0f);
        parentContainer.Parent.Parent.Append(TogglePanel);
        UIElement toggleElement2 = new()
        {
            Top = StyleDimension.FromPixelsAndPercent(0f, 0f),
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            HAlign = 1f
        };
        toggleElement2.SetPadding(0f);
        toggleElement2.PaddingTop = 8f;
        toggleElement2.PaddingBottom = 12f;
        TogglePanel.Append(toggleElement2);
        UIElement toggleElement3 = new UIElement
        {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            HAlign = 0.5f,
            VAlign = 0f
        };
        toggleElement3.SetPadding(10f);
        toggleElement3.PaddingBottom = 0f;
        toggleElement3.PaddingTop = 0f;
        toggleElement2.Append(toggleElement3);
        AddOptions(toggleElement3);
    }
    public void AddOptions(UIElement uiPanel)
    {
        int toggleSeparation = 8;

        int toggleCount = 0;

        var scrollBar = new UIScrollbar();
        scrollBar.SetView(200f, 1000f);
        scrollBar.Width.Set(20, 0);
        scrollBar.Height.Set(0, 0.8f);
        scrollBar.OverflowHidden = true;
        scrollBar.OnScrollWheel += HotbarScrollFix;
        scrollBar.HAlign = 1;

        UIToggleList toggleList = [];
        toggleList.Width.Set(-20, 1f);
        toggleList.Height.Set(0, 0.8f);
        toggleList.SetScrollbar(scrollBar);
        toggleList.OnScrollWheel += HotbarScrollFix;

        /*
        UIToggleList toggleList2 = [];
        toggleList2.Left.Set(ToggleWidth + 8, 0);
        toggleList2.Width.Set(0, 0.5f);
        toggleList2.Height.Set(0, 0.8f);
        toggleList2.SetScrollbar(scrollBar);
        toggleList2.OnScrollWheel += HotbarScrollFix;
        */

        uiPanel.Append(scrollBar);
        uiPanel.Append(toggleList);
        //uiPanel.Append(toggleList2);

        var elements = new List<UIElement>();
        foreach (UIWorldGenCategory category in ToggleCategories.Keys.OrderBy(x => x.Priority))
        {
            if (ToggleCategories[category].Count == 0)
                continue;
            category.Width = StyleDimension.FromPercent(1f);
            if (elements.Count % 2 == 1)
                elements.Add(new UIElement() { Height = StyleDimension.FromPixels(ToggleHeight) }); // empty space
            elements.Add(category);
            elements.Add(new UIElement() { Height = StyleDimension.FromPixels(ToggleHeight)}); // empty space

            foreach (var categToggle in ToggleCategories[category].OrderBy(i => i is UIWorldGenToggle t ? t.Priority : 1f))
                elements.Add(categToggle);
        }

        foreach (UIElement element in elements)
        {
            element.OnMouseOver += ShowToggleDescription;
            element.OnMouseOut += ClearToggleDescription;
            element.SetSnapPoint("WorldGenToggle" + toggleCount, 0);
            toggleCount++;
            toggleList.Add(element);
        }

        int height = 238; // evil magic number to put the separator in the same spot as the vanilla one

        UIHorizontalSeparator separator = new()
        {
            Width = StyleDimension.FromPercent(1f),
            Top = StyleDimension.FromPixels(height - 8f),
            Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
        };
        uiPanel.Append(separator);

        float num = 0f;
        UISlicedImage uISlicedImage = new UISlicedImage(Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelHighlight", (AssetRequestMode)1))
        {
            HAlign = 0.5f,
            VAlign = 1f,
            Width = StyleDimension.FromPixelsAndPercent((0f - num) * 2f, 1f),
            Left = StyleDimension.FromPixels(0f - num),
            Height = StyleDimension.FromPixelsAndPercent(40f, 0f),
            Top = StyleDimension.FromPixels(2f)
        };
        uISlicedImage.SetSliceDepths(10);
        uISlicedImage.Color = Color.LightGray * 0.7f;
        uiPanel.Append(uISlicedImage);
        UIText uIText = new(Language.GetText("UI.WorldDescriptionDefault"), 0.6f)
        {
            HAlign = 0f,
            VAlign = 0f,
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Top = StyleDimension.FromPixelsAndPercent(1f, 0f),
            PaddingLeft = 20f,
            PaddingRight = 20f,
            PaddingTop = 6f
        };
        uISlicedImage.Append(uIText);
        DescriptionText = uIText;
    }
    private void ShowToggleDescription(UIMouseEvent evt, UIElement listeningElement)
    {
        if (listeningElement is UIWorldGenToggle uiToggle)
        {
            LocalizedText localizedText = uiToggle.Description;
            DescriptionText.SetText(localizedText);
        }
        else if (listeningElement is UIWorldGenSlider uiSlider)
        {
            LocalizedText localizedText = uiSlider.Description;
            DescriptionText.SetText(localizedText);
        }
    }
    private void ClearToggleDescription(UIMouseEvent evt, UIElement listeningElement)
    {
        DescriptionText.SetText(Language.GetText("UI.WorldDescriptionDefault"));
    }
    private void ToggleMenu()
    {
        ModdedMenuActive = !ModdedMenuActive;
    }

    private void FinishCreatingWorld_Detour(On_UIWorldCreation.orig_FinishCreatingWorld orig, UIWorldCreation self)
    {
        foreach (var toggles in ToggleCategories.Values)
        {
            foreach (var toggle in toggles)
            {
                if (toggle is UIWorldGenToggle uiToggle)
                {
                    uiToggle.InvokeAction();
                }
                else if (toggle is UIWorldGenSlider uiSlider)
                {
                    uiSlider.InvokeAction();
                }
            }
        }
        orig(self);
    }
    private void Click_GoBack_Detour(On_UIWorldCreation.orig_Click_GoBack orig, UIWorldCreation self, UIMouseEvent evt, UIElement listeningElement)
    {
        if (ModdedMenuActive)
        {
            ModdedMenuActive = false;
            return;
        }
        orig(self, evt, listeningElement);
    }
    private void Click_NamingAndCreating_Detour(On_UIWorldCreation.orig_Click_NamingAndCreating orig, UIWorldCreation self, UIMouseEvent evt, UIElement listeningElement)
    {
        if (!ModdedMenuActive)
        {
            HasClickedMenu = true;
            ModdedMenuActive = true;
            return;
        }
        orig(self, evt, listeningElement);
    }

    private void MakeBackAndCreatebuttons_Detour(On_UIWorldCreation.orig_MakeBackAndCreatebuttons orig, UIWorldCreation self, UIElement outerContainer)
    {
        orig(self, outerContainer);
        foreach (var element in outerContainer.Children)
        {
            if (element is UITextPanel<LocalizedText> panel && panel.Text.Equals(Language.GetTextValue("UI.Create")))
            {
                CreateButton = panel;
            }
        }
    }

    private void HotbarScrollFix(UIScrollWheelEvent evt, UIElement listeningElement) => Main.LocalPlayer.ScrollHotbar(PlayerInput.ScrollWheelDelta / 120);

#region UIElement Detours
    // This exists to make the Info Menu dissappear when the modded tab is selected
    private void UIElement_Draw_Detour(On_UIElement.orig_Draw orig, UIElement self, SpriteBatch spriteBatch)
    {
        if (InfoMenuParent != null && ModdedMenuActive && self == InfoMenuParent?.Parent)
            return;
        if (!ModdedMenuActive && self == TogglePanel)
            return;
        if (self == CreateButton)
        {
            if (ModdedMenuActive)
                CreateButton.SetText(Language.GetText("UI.Create"));
            else
                CreateButton.SetText(Language.GetText("Mods.FargoSeeds.WorldGenMenu.Next"));
        }
        orig(self, spriteBatch);
    }
    private void UIElement_Update_Detour(On_UIElement.orig_Update orig, UIElement self, GameTime gameTime)
    {
        if (InfoMenuParent != null && ModdedMenuActive && self == InfoMenuParent?.Parent)
            return;
        if (!ModdedMenuActive && self == TogglePanel)
            return;
        orig(self, gameTime);
    }
    private bool UIElement_ContainsPoint_Detour(On_UIElement.orig_ContainsPoint orig, UIElement self, Vector2 point)
    {
        if (InfoMenuParent != null && ModdedMenuActive && self == InfoMenuParent?.Parent)
            return false;
        if (!ModdedMenuActive && self == TogglePanel)
            return false;
        return orig(self, point);
    }
#endregion
}