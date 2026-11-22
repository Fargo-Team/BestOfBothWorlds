using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace FargoSeeds.UI;

public class UISearchBar : UIPanel
{
    private sealed class BetterImageButton : UIElement
    {
        public Asset<Texture2D> Asset
        {
            get;

            set
            {
                field = value;

                Width.Set(value.Width(), 0f);
                Height.Set(value.Height(), 0f);
            }
        }

        public Asset<Texture2D>? HoverImage { get; set; }

        public float VisibilityActive { get; set; } = 1f;

        public float VisibilityInactive { get; set; } = 0.4f;

        public BetterImageButton(Asset<Texture2D> asset)
        {
            Asset = asset;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);

            SoundEngine.PlaySound(12);
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);

            // Automatically truncated.
            var dims = this.Dimensions;

            sb.Draw(
                Asset.Value,
                dims.TopLeft(),
                Color.White * (IsMouseHovering ? VisibilityActive : VisibilityInactive)
            );

            if (HoverImage is not null && IsMouseHovering)
            {
                sb.Draw(
                    HoverImage.Value,
                    dims.TopLeft(),
                    Color.White
                );
            }
        }
    }

    public readonly UIInputField Input;

    public UISearchBar(Func<string> hint)
    {
        _backgroundTexture = Assets.Images.UI.EmptyPanel.Asset;
        _borderTexture = Assets.Images.UI.SmallPanelOutline.Asset;

        Height.Set(30f, 0f);

        SetPadding(0f);

        PaddingLeft = 6;
        PaddingRight = 6f;

        const float button_margin = 22f;

        PaddingLeft += button_margin;
        PaddingRight += button_margin;

        Input = new UIInputField(hint);
        {
            Input.Width.Set(0f, 1f);
            Input.Height.Set(0f, 1f);

            Input.OnEscape += OnEscape_CancelText;
        }
        Append(Input);

        Assets.Images.UI.SearchButton.Asset.Wait();
        var searchButton = new BetterImageButton(Assets.Images.UI.SearchButton.Asset);
        {
            searchButton.VAlign = 0.5f;
            searchButton.Left = new StyleDimension(-button_margin, 0f);
        }
        Append(searchButton);

        Assets.Images.UI.SearchCancel.Asset.Wait();
        var searchCancelButton = new BetterImageButton(Assets.Images.UI.SearchCancel.Asset);
        {
            searchCancelButton.VAlign = 0.5f;
            searchCancelButton.HAlign = 1f;
            searchCancelButton.Left = new StyleDimension(button_margin, 0f);
            searchCancelButton.OnLeftClick += SearchCancelButton_CancelText;
        }
        Append(searchCancelButton);
    }

    public UISearchBar(string hint) : this(() => hint) { }

    public UISearchBar(LocalizedText text) : this(() => text.Value) { }

    private void OnEscape_CancelText(UIInputField input)
    {
        Input.Text = string.Empty;
    }

    private void SearchCancelButton_CancelText(UIMouseEvent evt, UIElement listeningElement)
    {
        Input.Text = string.Empty;
    }
}
