﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using GTA.Native;
using Font = GTA.UI.Font;

namespace SelectorWheel
{
    public static class UIHelper
    {
        public enum TextJustification
        {
            Center = 0,
            Left,
            Right //requires SET_TEXT_WRAP
        }

        public static void DrawCustomText(string Message, float FontSize, Font FontType,
            int Red, int Green, int Blue, int Alpha, float XPos, float YPos,
            int dropShawdowPixelDistance, int dRed, int dGreen, int dBlue, int dAlpha,
            TextJustification justifyType = TextJustification.Left, bool ForceTextWrap = false, float startWrap = 0f, float endWrap = 1f,
            bool withRectangle = false, int R = 0, int G = 0, int B = 0, int A = 255,
            float rectWidthOffset = 0f, float rectHeightOffset = 0f, float rectYPosDivisor = 23.5f)
        {
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "jamyfafi"); //Required, don't change this! AKA BEGIN_TEXT_COMMAND_DISPLAY_TEXT
            Function.Call(Hash.SET_TEXT_SCALE, FontSize, FontSize); //1st param: 1.0f
            Function.Call(Hash.SET_TEXT_FONT, (int)FontType);
            Function.Call(Hash.SET_TEXT_COLOUR, Red, Green, Blue, Alpha);
            Function.Call(Hash.SET_TEXT_DROPSHADOW, dropShawdowPixelDistance, dRed, dGreen, dBlue, dAlpha);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash.SET_TEXT_JUSTIFICATION, (int)justifyType);
            if (justifyType == TextJustification.Right || ForceTextWrap)
            {
                Function.Call(Hash.SET_TEXT_WRAP, startWrap, endWrap);
            }

            //Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, Message);
            AddLongString(Message);

            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, XPos, YPos); //AKA END_TEXT_COMMAND_DISPLAY_TEXT

            if (withRectangle)
            {
                switch (FontType)
                {
                    case Font.ChaletLondon: rectYPosDivisor = 15f; break;
                    case Font.HouseScript: rectYPosDivisor = 23f; break;
                    case Font.Monospace: rectYPosDivisor = 25f; break;
                    case Font.ChaletComprimeCologne: rectYPosDivisor = 30f; break;
                    case Font.Pricedown: rectYPosDivisor = 35f; break;
                }

                float adjWidth = MeasureStringWidthNoConvert(Message, FontType, FontSize);
                float fontHeight = MeasureFontHeightNoConvert(FontSize, FontType);
                float rectangleWidth = (endWrap - startWrap) + rectWidthOffset;
                float baseYPos = YPos + (FontSize / rectYPosDivisor);
                //int numLines = (int)Math.Ceiling(adjWidth / ((endWrap - startWrap) * 0.98f));
                int numLines = GetStringLineCount(Message, FontSize, FontType, startWrap, endWrap, XPos, YPos);
                for (int i = 0; i < numLines; i++)
                {
                    float adjustedYPos = i == 0 ? baseYPos - rectHeightOffset / 2 
                        : (i == numLines - 1 ? baseYPos + rectHeightOffset / 2 
                        : baseYPos);

                    float adjustedRectangleHeight = i == 0 || i == numLines - 1 ? fontHeight + rectHeightOffset 
                        : fontHeight;

                    float adjustedXPos = justifyType == TextJustification.Left ? XPos + ((endWrap - startWrap) / 2) 
                        : (justifyType == TextJustification.Right ? endWrap - ((endWrap - startWrap) / 2) 
                        : XPos);

                    DrawRectangle(adjustedXPos, adjustedYPos + (i * fontHeight), rectangleWidth, adjustedRectangleHeight, R, G, B, A);
                }
            }
        }

        public static void DrawRectangle(float BgXpos, float BgYpos, float BgWidth, float BgHeight, int bgR, int bgG, int bgB, int bgA)
        {
            Function.Call(Hash.DRAW_RECT, BgXpos, BgYpos, BgWidth, BgHeight, bgR, bgG, bgB, bgA);
        }

        public static void AddLongString(string str)
        {
            const int strLen = 99;
            for (int i = 0; i < str.Length; i += strLen)
            {
                string substr = str.Substring(i, Math.Min(strLen, str.Length - i));
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, substr); //ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME
            }
        }

        public static float MeasureStringWidth(string str, Font font, float fontsize)
        {
            //int screenw = 2560;// Game.ScreenResolution.Width;
            //int screenh = 1440;// Game.ScreenResolution.Height;
            const float height = 1080f;
            float ratio = (float)GTA.UI.Screen.Resolution.Width / GTA.UI.Screen.Resolution.Height;
            float width = height * ratio;
            return MeasureStringWidthNoConvert(str, font, fontsize) * width;
        }

        private static float MeasureStringWidthNoConvert(string str, Font font, float fontsize)
        {
            Function.Call((Hash)0x54CE8AC98E120CAB, "jamyfafi"); //_BEGIN_TEXT_COMMAND_WIDTH
            AddLongString(str);
            Function.Call(Hash.SET_TEXT_FONT, (int)font);
            Function.Call(Hash.SET_TEXT_SCALE, fontsize, fontsize);
            return Function.Call<float>((Hash)0x85F061DA64ED2F67, true); //_END_TEXT_COMMAND_GET_WIDTH //Function.Call<float>((Hash)0x85F061DA64ED2F67, (int)font) * fontsize; //_END_TEXT_COMMAND_GET_WIDTH
        }

        public static float MeasureFontHeight(float fontSize, Font font)
        {
            return Function.Call<float>(Hash.GET_RENDERED_CHARACTER_HEIGHT, fontSize, (int)font) * GTA.UI.Screen.Resolution.Height; //1080f
        }

        public static float MeasureFontHeightNoConvert(float fontSize, Font font)
        {
            return Function.Call<float>(Hash.GET_RENDERED_CHARACTER_HEIGHT, fontSize, (int)font);
        }

        public static int GetStringLineCount(string text, float FontSize, Font FontType, float startWrap, float endWrap, float x, float y)
        {
            Function.Call((Hash)0x521FB041D93DD0E4, "jamyfafi"); //_BEGIN_TEXT_COMMAND_LINE_COUNT
            Function.Call(Hash.SET_TEXT_SCALE, FontSize, FontSize); //1st param: 1.0f
            Function.Call(Hash.SET_TEXT_FONT, (int)FontType);
            Function.Call(Hash.SET_TEXT_WRAP, startWrap, endWrap);
            AddLongString(text);
            return Function.Call<int>((Hash)0x9040DFB09BE75706, x, y); //_END_TEXT_COMMAND_GET_LINE_COUNT
        }

        public static float XPixelToPercentage(int pixel)
        {
            const float height = 1080f;
            float ratio = (float)GTA.UI.Screen.Resolution.Width / GTA.UI.Screen.Resolution.Height;
            float width = height * ratio;

            return pixel / width;
        }

        public static float YPixelToPercentage(int pixel)
        {
            const float height = 1080f;
            float ratio = (float)GTA.UI.Screen.Resolution.Width / GTA.UI.Screen.Resolution.Height;
            float width = height * ratio;

            return pixel / height;
        }

        public static float XPercentageToPixel(float percent)
        {
            const float height = 1080f;
            float ratio = (float)GTA.UI.Screen.Resolution.Width / GTA.UI.Screen.Resolution.Height;
            float width = height * ratio;

            return percent * width;
        }

        public static float YPercentageToPixel(float percent)
        {
            const float height = 1080f;
            float ratio = (float)GTA.UI.Screen.Resolution.Width / GTA.UI.Screen.Resolution.Height;
            float width = height * ratio;

            return percent * height;
        }
        
        public static float AspectRatio { get; private set; } = Function.Call<float>(Hash.GET_SCREEN_ASPECT_RATIO, true);

        public static float UpdateAspectRatio()
        {
            AspectRatio = Function.Call<float>(Hash.GET_SCREEN_ASPECT_RATIO, true);
            return AspectRatio;
        }

        public static PointF PointFromCenter(float x, float y, float normalizedAngle)
        {
            // Credits to MaxShadow for this method
            float angle2 = (normalizedAngle * (float)Math.PI * 2) - ((float)Math.PI / 2);
            float x2 = (GTA.UI.Screen.Width / 2) + (float)Math.Cos(angle2) * x;
            float y2 = (GTA.UI.Screen.Height / 2) + (float)Math.Sin(angle2) * y * (AspectRatio / (16f / 9f));
            return new PointF(x2, y2);
        }
    }
}