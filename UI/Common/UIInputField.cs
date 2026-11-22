using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using ReLogic.Localization.IME;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace FargoSeeds.UI;

// Based on my prior input field.
// See: https://github.com/gold-meridian/daybreak-mod/blob/feat/assembly-split/src/Daybreak.UI.V1/InputField.cs 
// TODO: Support multiline text, IMEs, and text selection.
/// <summary>
///     An interface panel accepting complex text input.
/// </summary>
public class UIInputField : UIElement
{
    /// <summary>
    ///     The scale of the text in the field.
    /// </summary>
    public float TextScale { get; set; } = 1f;

    /// <summary>
    ///     The text within the field.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    ///     The hint to display when there is no text.
    /// </summary>
    public Func<string> Hint { get; }

    /// <summary>
    ///     The maximum number of characters allowed to be entered.
    /// </summary>
    public int MaxChars { get; set; } = 100;

    /// <summary>
    ///     Weither Terraria's TextSnippets will be used.
    /// </summary>
    public bool AllowChatTags { get; set; } = false;

    /// <summary>
    ///     Any characters which may not be entered into the field.
    /// </summary>
    public HashSet<char> BlacklistedChars { get; } = [];

    /// <summary>
    ///     If populated, only these characters will be allowed to be entered in
    ///     the field.  Any characters that are also in
    ///     <see cref="BlacklistedChars"/> will still be ignored.
    /// </summary>
    public HashSet<char> WhitelistedChars { get; } = [];

    /// <summary>
    ///     The horizontal alignment of text within the field, represented as a
    ///     [0,1] float serving as a percentage.
    /// </summary>
    public float TextAlignX { get; set; }

    /// <summary>
    ///     Ran when typing is "accepted" via either clicking off or pressing Enter.
    /// </summary>
    public event Action<UIInputField>? OnEnter;

    /// <summary>
    ///     Ran when typing is "cancelled" via pressing Escape.
    /// </summary>
    public event Action<UIInputField>? OnEscape;

    /// <summary>
    ///     Ran every time the text is modified.
    /// </summary>
    public event Action<UIInputField>? OnTextChanged;

    private int cursorPosition;
    private bool currentlyWriting;

    // The text from before writing started.
    private string? lastText;
    private string? oldText;

    /// <summary>
    ///     Initializes this input field with some default styling.
    /// </summary>
    public UIInputField(Func<string> hint)
    {
        Hint = hint;

        Width.Set(0f, 1f);
        Height.Set(30f, 0f);

        SetPadding(0f);

        PaddingLeft = 6f;
        PaddingRight = 6f;
    }

    /// <inheritdoc />
    public UIInputField(string hint) : this(() => hint) { }

    /// <inheritdoc />
    public UIInputField(LocalizedText text) : this(() => text.Value) { }

    /// <inheritdoc />
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        if (evt.Target != this || !this.InnerDimensions.Contains(evt.MousePosition.ToPoint()))
        {
            currentlyWriting = false;
        }
    }

    /// <inheritdoc />
    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);

        Main.ClosePlayerChat();

        InputHelpers.SyncBlinkerStartTime();
        currentlyWriting = true;
        lastText = Text;

        if (Text.Length <= 0)
        {
            InputHelpers.CursorPositon = 0;

            return;
        }

        var dims = this.InnerDimensions;

        var font = FontAssets.MouseText.Value;

        var textSize =
            AllowChatTags
            ? ChatManager.GetStringSize(font, Text, Vector2.One)
            : font.MeasureString(Text);

        var origin = new Vector2(textSize.X * TextAlignX, textSize.Y * 0.5f);

        cursorPosition = font.GetHoveredCharacter(
            Text,
            Main.MouseScreen,
            CalculateTextPositon(font, Text, dims),
            origin,
            new Vector2(TextScale),
            AllowChatTags
        );

        InputHelpers.CursorPositon = cursorPosition;
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Text != oldText && oldText is not null)
        {
            OnTextChanged?.Invoke(this);
        }

        oldText = Text;

        var cap = MaxChars;
        if (Text.Length > cap)
        {
            Text = Text[..cap];

            cursorPosition = Math.Min(cursorPosition, cap);
        }

        var clickedOff = !Main.hasFocus || (Main.mouseLeft && !IsMouseHovering); // FocusHelper.AllowInputProcessing
        if (clickedOff && currentlyWriting)
        {
            OnEnter?.Invoke(this);
            currentlyWriting = false;
        }
    }

    private void HandleInput()
    {
        InputHelpers.WritingText = true;

        var cancellationType = InputHelpers.GetInput(
            Text,
            out var newText,
            false,
            AllowChatTags,
            BlacklistedChars,
            WhitelistedChars
        );
        switch (cancellationType)
        {
            case InputCancellationType.Confirmed:
                Text = newText;
                OnEnter?.Invoke(this);
                currentlyWriting = false;
                break;

            case InputCancellationType.Escaped:
                Text = lastText ?? string.Empty;
                OnEscape?.Invoke(this);
                currentlyWriting = false;
                break;

            case InputCancellationType.None:
            default:
                Text = newText;
                break;
        }
    }

    /// <inheritdoc />
    protected override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);

        sb.End();

        var priorState = sb.GraphicsDevice.RasterizerState;
        var priorScissor = sb.GraphicsDevice.ScissorRectangle;

        sb.GraphicsDevice.ScissorRectangle = GetClippingRectangle(sb);

        var dims = this.InnerDimensions;

        var cursorMargin = 5f * TextScale;
        {
            dims.Width -= (int)cursorMargin;
        }

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);
        {
            var hint = Hint();

            var text = Text == string.Empty ? hint : Text;

            var font = FontAssets.MouseText.Value;
            var position = CalculateTextPositon(font, text, dims);

            var textSize = font.MeasureString(text, AllowChatTags);

            var origin = new Vector2(textSize.X * TextAlignX, textSize.Y * 0.5f);

            if (Text == string.Empty)
            {
                ChatManager.DrawColorCodedStringWithShadow(
                    sb,
                    font,
                    hint,
                    position,
                    Color.Gray,
                    0f,
                    origin,
                    new Vector2(TextScale)
                );
            }

            var cursorIndex = Math.Min(InputHelpers.CursorPositon, Text.Length);

            sb.DrawInputStringWithShadow(
                font,
                Text,
                position,
                Color.White,
                origin,
                new Vector2(TextScale),
                currentlyWriting,
                cursorIndex,
                AllowChatTags
            );
        }
        sb.End();

        sb.GraphicsDevice.ScissorRectangle = priorScissor;

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, priorState, null, Main.UIScaleMatrix);

        if (currentlyWriting)
        {
            HandleInput();
        }
    }

    private Vector2 CalculateTextPositon(DynamicSpriteFont font, string text, Rectangle dims)
    {
        var position = new Vector2(dims.X + dims.Width * TextAlignX + 2f, dims.Y + dims.Height * 0.5f + 4);

        var textSize = font.MeasureString(text, AllowChatTags);

        var origin = new Vector2(textSize.X * TextAlignX, textSize.Y * 0.5f);

        var cursorIndex = Math.Min(InputHelpers.CursorPositon, Text.Length);

        if (!currentlyWriting || textSize.X * TextScale < dims.Width)
        {
            return position;
        }

        var cursorPos =
            AllowChatTags
                ? ChatManager.GetStringSize(font, text[..cursorIndex], Vector2.One).X
                : font.MeasureString(text[..cursorIndex]).X;

        cursorPos -= origin.X;

        var offset = cursorPos * TextScale;

        // Each half of the text separated by the alignment
        var width =
            Math.Sign(offset) <= 0
                ? (dims.Width * TextAlignX)
                : (dims.Width * (1f - TextAlignX));

        offset = Utils.Remap(Math.Abs(offset), width, textSize.X * TextScale, 0f, (textSize.X * TextScale) - width) * Math.Sign(offset);
        {
            position.X -= offset;
        }

        return position;
    }
}

public enum InputCancellationType : byte
{
    None,
    Escaped,
    Confirmed,
}

/// <summary>
///     Simpler input system based on Vanilla's <see cref="Main.GetInputText"/>
///     with logic for input cancellation via the
///     <see cref="InputCancellationType"/> enum.
/// </summary>
public static class InputHelpers
{
    private const int max_stroke_length = 20;

    private const int key_timer_delay = 45;

    private static readonly char[] invalid_chars =
    [
        .. Enumerable.Range('\x0', '\x1F' + 1).Select(i => (char)i),  // Invisible characters from Null to Unit Separator, stopping before Space
        '\x7F',                                                       // Delete
    ];

    private static int leftArrowTimer = key_timer_delay;

    private static int rightArrowTimer = key_timer_delay;

    private static int backspaceTimer = key_timer_delay;

    private static int deleteTimer = key_timer_delay;

    private static readonly StringBuilder keyStroke = new StringBuilder();

    // Stores the state of WritingText for use outside of drawing scopes
    private static bool wasWritingText;

    private static float blinkerStartTime;

    /// <summary>
    ///     If text is currently being written using this system.
    /// </summary>
    public static bool WritingText
    {
        get => PlayerInput.WritingText;
        set
        {
            wasWritingText |= value;
            PlayerInput.WritingText = value;
        }
    }

    /// <summary>
    ///     The index of the "cursor" where written text is inserted.
    /// </summary>
    public static int CursorPositon { get; set; }

    private sealed class Loader : ILoadable
    {
        void ILoadable.Load(Mod mod) => Load();

        void ILoadable.Unload() => Unload();
    }

    private static void Load()
    {
        On_PlayerInput.UpdateInput += UpdateInput_UpdateWasWritingText;
        On_UILinksInitializer.FancyExit += FancyExit_IgnoreExitIfWriting;
        On_Main.DoUpdate_Enter_ToggleChat += DoUpdate_Enter_ToggleChat_BlockInput;
        Platform.Get<IImeService>().AddKeyListener(OnKeyStroke);
    }

    private static void Unload()
    {
        Platform.Get<IImeService>().RemoveKeyListener(OnKeyStroke);
    }

    private static void UpdateInput_UpdateWasWritingText(On_PlayerInput.orig_UpdateInput orig)
    {
        wasWritingText = WritingText;

        orig();
    }

    private static void FancyExit_IgnoreExitIfWriting(On_UILinksInitializer.orig_FancyExit orig)
    {
        if (!wasWritingText)
        {
            orig();
        }
    }

    private static void DoUpdate_Enter_ToggleChat_BlockInput(On_Main.orig_DoUpdate_Enter_ToggleChat orig)
    {
        Main.chatRelease &= !wasWritingText;

        orig();
    }

    private static void OnKeyStroke(char key)
    {
        if (WritingText &&
            keyStroke.Length <= max_stroke_length)
        {
            keyStroke.Append(key);
        }
    }

    /// <summary>
    ///     Extended version of <see cref="Main.GetInputText(string, bool)"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <param name="allowLineBreaks"></param>
    /// <param name="allowChatTags"></param>
    /// <param name="blacklistedChars"></param>
    /// <param name="whitelistedChars"></param>
    /// <returns></returns>
    public static InputCancellationType GetInput(
        string input,
        out string output,
        bool allowLineBreaks = false,
        bool allowChatTags = false,
        IEnumerable<char>? blacklistedChars = null,
        IEnumerable<char>? whitelistedChars = null)
    {
        output = input;

        Main.oldInputText = Main.inputText;
        Main.inputText = Keyboard.GetState();

        WritingText &= Main.hasFocus; // FocusHelper.AllowInputProcessing

        if (!WritingText)
        {
            return InputCancellationType.None;
        }

        Main.instance.HandleIME();

        blacklistedChars ??= [];
        blacklistedChars = blacklistedChars.Concat(invalid_chars);

        // Arrow key movement
        {
            var left = 1;
            var right = 1;

            if (allowChatTags)
            {
                TryGetNeighborSnippets(input, out var previous, out var next);

                left = previous?.DeleteWhole is true ? previous.TextOriginal.Length : left;
                right = next?.DeleteWhole is true ? next.TextOriginal.Length : right;
            }

            // Left
            if (Keys.Left.Held)
            {
                leftArrowTimer--;
            }
            else
            {
                leftArrowTimer = key_timer_delay;
            }

            if (Keys.Left.JustPressed
             || (Keys.Left.Held
              && leftArrowTimer <= 0))
            {
                CursorPositon -= left;
            }

            // Right
            if (Keys.Right.Held)
            {
                rightArrowTimer--;
            }
            else
            {
                rightArrowTimer = key_timer_delay;
            }

            if (Keys.Right.JustPressed
             || (Keys.Right.Held
              && rightArrowTimer <= 0))
            {
                CursorPositon += right;
            }
        }

        // Home/End
        {
            if (Keys.Home.JustPressed)
            {
                CursorPositon = 0;
            }

            if (Keys.End.JustPressed)
            {
                CursorPositon = output.Length;
            }
        }

        CursorPositon = Math.Clamp(CursorPositon, 0, output.Length);

        // Special actions
        {
            var controlPressed =
                (Keys.LeftControl.Pressed
              || Keys.RightControl.Pressed)
             && !Keys.LeftAlt.Pressed
             && !Keys.RightAlt.Pressed;

            var shiftPressed =
                Keys.LeftShift.Pressed
             || Keys.RightShift.Pressed;

            // Clear
            if (controlPressed
             && Keys.Z.JustPressed)
            {
                output = string.Empty;
                CursorPositon = 0;
            }
            // Cut
            else if ((controlPressed
              && Keys.X.JustPressed)
             || (shiftPressed
              && Keys.Delete.JustPressed))
            {
                Platform.Get<IClipboard>().Value = output;
                output = string.Empty;
                CursorPositon = 0;
            }
            // Copy
            else if (Keys.C.JustPressed)
            {
                Platform.Get<IClipboard>().Value = output;
            }
            // Paste
            else if ((controlPressed
              && Keys.V.JustPressed)
             || (shiftPressed
              && Keys.Insert.JustPressed))
            {
                var paste = RemoveInvalidCharacters(GetPaste(output, allowLineBreaks));

                output = output.Insert(CursorPositon, paste);

                CursorPositon += paste.Length;
            }
        }

        // Input
        {
            if (keyStroke.Length >= 1)
            {
                var stroke = RemoveInvalidCharacters(keyStroke.ToString());

                keyStroke.Clear();

                output = output.Insert(CursorPositon, stroke);

                CursorPositon += stroke.Length;
            }
        }

        // Backspace
        {
            if (Keys.Back.Held)
            {
                backspaceTimer--;
            }
            else
            {
                backspaceTimer = key_timer_delay;
            }
            if ((Keys.Back.JustPressed
              || (Keys.Back.Held
               && backspaceTimer <= 0))
             && output.Length >= 1
             && CursorPositon >= 1)
            {
                output = string.Concat(output.AsSpan(0, CursorPositon - 1),
                    output.AsSpan(CursorPositon, output.Length - CursorPositon));

                CursorPositon--;
            }
        }

        {
            if (Keys.Delete.Held)
            {
                deleteTimer--;
            }
            else
            {
                deleteTimer = key_timer_delay;
            }

            if ((Keys.Delete.JustPressed
              || (Keys.Delete.Held
              && deleteTimer <= 0))
             && CursorPositon < output.Length)
            {
                output = string.Concat(
                    output.AsSpan(0, CursorPositon),
                    output.AsSpan(CursorPositon + 1, output.Length - CursorPositon - 1)
                );
            }
        }

        // Escapes
        {
            // Unsure of why vanilla checks if you're on Windows before allowing escape inputs
            // if (!Platform.IsWindows && Main.inputText.IsKeyDown(Keys.Escape) && !Main.oldInputText.IsKeyDown(Keys.Escape))
            if (Keys.Escape.JustPressed)
            {
                // Definitely sketchy, but is designed to prevent UI from vanishing whilst typing
                PlayerInput.WritingText = false;
                return InputCancellationType.Escaped;
            }

            if (Keys.Enter.JustPressed)
            {
                WritingText = false;
                return InputCancellationType.Confirmed;
            }
        }

        return InputCancellationType.None;

        static void TryGetNeighborSnippets(string input, out TextSnippet? previous, out TextSnippet? next)
        {
            var snippets = ChatManager.ParseMessage(input, Color.White);

            previous = null;
            next = null;

            var length = 0;

            foreach (var snippet in snippets)
            {
                if (length + snippet.TextOriginal.Length > CursorPositon)
                {
                    next = snippet;

                    if (CursorPositon > length)
                    {
                        previous = snippet;
                    }

                    break;
                }

                length += snippet.TextOriginal.Length;

                previous = snippet;
            }
        }

        string RemoveInvalidCharacters(string input)
        {
            foreach (var c in blacklistedChars)
            {
                input = input.Replace(c.ToString(), string.Empty);
            }

            if (whitelistedChars is not null &&
                whitelistedChars.Any())
            {
                input = new string(input.Where(whitelistedChars.Contains).ToArray());
            }

            return input;
        }
    }

    private static string GetPaste(string input, bool allowLineBreaks)
    {
        var clipboard = Platform.Get<IClipboard>();

        return input.Insert(CursorPositon, allowLineBreaks ? clipboard.MultiLineValue : clipboard.Value);
    }

    public static void DrawInputString(
        this SpriteBatch spriteBatch,
        DynamicSpriteFont font,
        string text,
        Vector2 position,
        Color color,
        Vector2 origin,
        Vector2 scale,
        bool drawBlinker = false,
        int blinkerIndex = -1,
        bool allowChatTags = false
    )
    {
        spriteBatch.DrawInputStringWithShadow(
            font,
            text,
            position,
            color,
            Color.Black,
            origin,
            scale,
            drawBlinker,
            blinkerIndex,
            allowChatTags,
            -1f
        );
    }

    public static void DrawInputStringWithShadow(
        this SpriteBatch sb,
        DynamicSpriteFont font,
        string text,
        Vector2 position,
        Color color,
        Vector2 origin,
        Vector2 scale,
        bool drawBlinker = false,
        int blinkerIndex = -1,
        bool allowChatTags = false
    )
    {
        sb.DrawInputStringWithShadow(
            font,
            text,
            position,
            color,
            Color.Black,
            origin,
            scale,
            drawBlinker,
            blinkerIndex,
            allowChatTags
        );
    }

    public static void DrawInputStringWithShadow(
        this SpriteBatch sb,
        DynamicSpriteFont font,
        string text,
        Vector2 position,
        Color color,
        Color shadowColor,
        Vector2 origin,
        Vector2 scale,
        bool drawBlinker = false,
        int blinkerIndex = -1,
        bool allowChatTags = false,
        float spread = 2f
    )
    {
        DrawStringWithShadow(
            text,
            position,
            origin
        );

        // Blinker
        {
            const float blink_duration_in_seconds = 2f / 3f;
            const float blink_percent = 0.5f;

            var blinkTimer = MathF.Max(0f, (Main.GlobalTimeWrappedHourly - blinkerStartTime) % blink_duration_in_seconds);

            if (drawBlinker && blinkerIndex != -1 && blinkTimer < blink_duration_in_seconds * blink_percent)
            {
                const float blinker_x_offset = -2f;

                var blinkerX =
                    allowChatTags
                    ? ChatManager.GetStringSize(font, text[..blinkerIndex], Vector2.One).X
                    : font.MeasureString(text[..blinkerIndex]).X;

                blinkerX += blinker_x_offset;

                var blinkerOffset = new Vector2(blinkerX, 0) - origin;
                blinkerOffset *= scale;

                DrawStringWithShadow(
                    "|",
                    position + blinkerOffset,
                    Vector2.Zero
                );
            }
        }

        return;

        void DrawStringWithShadow(
            string text,
            Vector2 position,
            Vector2 origin
        )
        {
            if (allowChatTags)
            {
                ChatManager.DrawColorCodedStringWithShadow(
                    sb,
                    font,
                    text,
                    position - (origin * scale),
                    color,
                    shadowColor,
                    0f,
                    Vector2.Zero,
                    scale,
                    -1,
                    spread
                );

                return;
            }

            if (spread > 0f)
            {
                foreach (var dir in ChatManager.ShadowDirections)
                {
                    sb.DrawString(
                        font,
                        text,
                        position + dir * spread,
                        shadowColor,
                        0f,
                        origin,
                        scale,
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            sb.DrawString(
                font,
                text,
                position,
                color,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }
    }

    public static int GetHoveredCharacter(
        this DynamicSpriteFont font,
        string text,
        Vector2 mousePosition,
        Vector2 position,
        Vector2 origin,
        Vector2 scale,
        bool allowChatTags = false
    )
    {
        mousePosition -= position;
        mousePosition -= origin;
        mousePosition /= scale;

        if (allowChatTags)
        {
            var snippets = ChatManager.ParseMessage(text, Color.White);

            // TODO: Figure out why this is done
            var totalWidth = -origin.X * 2;

            var charCount = 0;

            foreach (var snippet in snippets)
            {
                snippet.UniqueDraw(true, out var snippetSize, Main.spriteBatch);

                totalWidth += snippetSize.X;

                if (totalWidth <= mousePosition.X)
                {
                    charCount += snippet.TextOriginal.Length;

                    continue;
                }

                if (snippet.DeleteWhole)
                {
                    return charCount + snippet.TextOriginal.Length;
                }
                if (FindHoveredCharacter(charCount, snippet.TextOriginal.Length, out var index))
                {
                    return index;
                }

                break;
            }
        }
        else if (FindHoveredCharacter(0, text.Length, out var index))
        {
            return index;
        }

        return text.Length;

        bool FindHoveredCharacter(int start, int length, out int index)
        {
            var first = true;
            var lastKerning = 0f;

            var totalWidth = -origin.X * 2;

            if (start > 0)
            {
                totalWidth +=
                    allowChatTags
                    ? ChatManager.GetStringSize(font, text[..start], Vector2.One).X
                    : font.MeasureString(text[..start]).X;
            }

            index = 0;

            for (var i = start; i < start + length; i++)
            {
                var c = text[i];

                var charSize = font.MeasureChar(c, first, Vector2.One, lastKerning, out lastKerning);

                if (mousePosition.X >= totalWidth && mousePosition.X <= totalWidth + charSize.X)
                {
                    index = mousePosition.X >= totalWidth + (charSize.X * .5f) ? i + 1 : i;

                    return true;
                }

                totalWidth += charSize.X;
                first = false;
            }

            return false;
        }
    }

    /// <summary>
    ///     Wraps <see cref="ChatManager.GetStringSize(DynamicSpriteFont, string, Vector2, float)"/> and <see cref="DynamicSpriteFont.MeasureString(string)"/>
    ///     based on <paramref name="allowChatTags"/>.
    /// </summary>
    /// <param name="font"></param>
    /// <param name="text"></param>
    /// <param name="allowChatTags">
    ///     If <see cref="ChatManager.GetStringSize(DynamicSpriteFont, string, Vector2, float)"/> should be used over <see cref="DynamicSpriteFont.MeasureString(string)"/>
    /// </param>
    /// <returns>
    ///     The size of the string in the font as a <see cref="Vector2"/>.
    /// </returns>
    public static Vector2 MeasureString(this DynamicSpriteFont font, string text, bool allowChatTags)
    {
        var textSize =
            allowChatTags
          ? ChatManager.GetStringSize(font, text, Vector2.One)
          : font.MeasureString(text);

        // Prevents the blinker from appearing offset vertically
        if (textSize.Y <= 0)
        {
            textSize.Y = font.MeasureChar(font.DefaultCharacter, false, Vector2.One, 0f, out _).Y;
        }

        return textSize;
    }

    // TODO: Should this be provided? Should this and above string rendering utils be moved?
    private static Vector2 MeasureChar(this DynamicSpriteFont font, char c, bool firstChar, Vector2 scale, float lastKerning, out float kerningZ)
    {
        var output = Vector2.Zero;
        output.Y = font.LineSpacing;

        var characterData = font.GetCharacterData(c);
        var kerning = characterData.Kerning;

        if (firstChar)
        {
            kerning.X = Math.Max(kerning.X, 0f);
        }
        else
        {
            output.X += font.CharacterSpacing + lastKerning;
        }

        output.X += kerning.X + kerning.Y;

        output.Y = Math.Max(output.Y, characterData.Padding.Height);

        output.X += Math.Max(kerning.Z, 0f);

        output *= scale;

        kerningZ = kerning.Z;

        kerningZ *= scale.X;

        return output;
    }

    public static void SyncBlinkerStartTime()
    {
        blinkerStartTime = Main.GlobalTimeWrappedHourly;
    }

    extension(Keys key)
    {
        public bool Pressed =>
            Main.inputText.IsKeyDown(key);

        public bool Held =>
            Main.inputText.IsKeyDown(key) &&
            Main.oldInputText.IsKeyDown(key);

        public bool JustPressed =>
            Main.inputText.IsKeyDown(key) &&
            !Main.oldInputText.IsKeyDown(key);

        public bool JustReleased =>
            !Main.inputText.IsKeyDown(key) &&
            Main.oldInputText.IsKeyDown(key);
    }
}
