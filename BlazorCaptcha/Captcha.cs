﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BlazorCaptcha;

public class Captcha : ComponentBase
{

    [Parameter]
    public int Width { get; set; } = 170;

    [Parameter]
    public int Height { get; set; } = 40;

    [Parameter]
    public int CharNumber { get; set; } = 5;

    [Parameter]
    public EventCallback<MouseEventArgs> OnRefresh { get; set; }

    private string _captchaWord;
    [Parameter]
    public string CaptchaWord {
        get
        {
            return _captchaWord;
        }
        set {
            if (_captchaWord != value )
            {
                _captchaWord = value;
                Initialization();
            }
        }
    }

    [Parameter]
    public EventCallback<string> CaptchaWordChanged { get; set; }

    private async Task OnRefreshInternal()
    {
        CaptchaWord = Tools.GetCaptchaWord(CharNumber);
        Initialization();
        await CaptchaWordChanged.InvokeAsync(CaptchaWord);
    }

    private Random RandomValue { get; set; }
    private List<Letter> Letters;
    private SKColor _bgColor;

    public Captcha()
    {
        Initialization();
    }


    private void Initialization()
    {
        if (string.IsNullOrEmpty(CaptchaWord)) return;

        RandomValue = new Random();

        _bgColor = new SKColor((byte)RandomValue.Next(90, 130), (byte)RandomValue.Next(90, 130), (byte)RandomValue.Next(90, 130));

        var fontFamilies = new string[] { "Courier", "Arial", "Verdana", "Times New Roman" };

        Letters = new List<Letter>();

        if (!string.IsNullOrEmpty(CaptchaWord))
        {
            foreach (char c in CaptchaWord)
            {
                var letter = new Letter
                {
                    Value = c.ToString(),
                    Angle = RandomValue.Next(-15, 25),
                    ForeColor = new SKColor((byte)RandomValue.Next(256), (byte)RandomValue.Next(256), (byte)RandomValue.Next(256)),
                    Family = fontFamilies[RandomValue.Next(0, fontFamilies.Length)],
                };

                Letters.Add(letter);
            }
        }

    }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (RandomValue == null) return;
        if (string.IsNullOrEmpty(CaptchaWord)) return;

        string img = "";
        SKImageInfo imageInfo = new(Width, Height);
        using (var surface = SKSurface.Create(imageInfo))
        {
            var canvas = surface.Canvas;
            canvas.Clear(_bgColor);

            using (SKPaint paint = new())
            {
                float x = 10;

                foreach (Letter l in Letters)
                {
                    paint.Color = l.ForeColor;
                    paint.Typeface = SKTypeface.FromFamilyName(l.Family);
                    paint.TextAlign = SKTextAlign.Left;
                    paint.TextSize = RandomValue.Next(Height / 2, (Height / 2) + (Height / 4));
                    paint.FakeBoldText = true;
                    paint.IsAntialias = true;

                    SKRect rect = new();
                    float width = paint.MeasureText(l.Value, ref rect);
                    float textWidth = width - 2;// + rect.Right;
                    var y = ((Height - rect.Height) / 2);


                    if (l.Angle < -5)
                    {
                        y = Height - rect.Height;
                    }
                    canvas.Save();
                    canvas.Translate(x, y);
                    canvas.RotateDegrees(l.Angle);
                    canvas.DrawText(l.Value, x, y, paint);
                    canvas.Restore();

                    x += textWidth;
                }

                canvas.DrawLine(0, RandomValue.Next(0, Height), Width, RandomValue.Next(0, Height), paint);
                canvas.DrawLine(0, RandomValue.Next(0, Height), Width, RandomValue.Next(0, Height), paint);
                paint.Style = SKPaintStyle.Stroke;
                canvas.DrawOval(RandomValue.Next(-Width, Width), RandomValue.Next(-Height, Height), Width, Height, paint);
            }


            // save the file
            MemoryStream memoryStream = new();
            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 75))
                data.SaveTo(memoryStream);
            string imageBase64Data2 = Convert.ToBase64String(memoryStream.ToArray());
            img = string.Format("data:image/gif;base64,{0}", imageBase64Data2);
        }

        //---


        var seq = 0;
        builder.OpenElement(++seq, "div");
        builder.AddAttribute(++seq, "class", "divCaptach");
        {
            builder.OpenElement(++seq, "img");
            builder.AddAttribute(++seq, "src", img);
            builder.CloseElement(); 

            builder.OpenElement(++seq, "button");
            {
                builder.AddAttribute(++seq, "class", "btn-refresh");
                builder.AddAttribute(++seq, "type", "button");
                builder.AddAttribute(++seq, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnRefreshInternal()));
            }
            builder.CloseElement(); 
        }
        builder.CloseElement();


        base.BuildRenderTree(builder);
    }

}




