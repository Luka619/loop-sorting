# UI Prompt Sheet (v0.5 / Creamy Plastic) - All

Usage: copy each item prompt to generate a PNG with the exact same filename and pixel size, then run `Tools/UiRestyleV05/ReplacePngs.ps1` to overwrite Unity assets (only `.png`, keep `.meta`).

Global constants (recommended):

**STYLE_CORE**
~~~
soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**NEGATIVE_CORE**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

**EXPORT_CORE**
~~~
centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG
~~~

---

## UI_Sprites/badge_red_bg.png (152x162)
- template: BADGE_BG

**Positive prompt**
~~~
small notification badge background, red candy plastic, round shape, thick outline, glossy highlight, soft inner shadow, no text, exact 152x162px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/bg_main.png (1080x1920)
- template: BG_MAIN

**Positive prompt**
~~~
warm creamy background gradient, subtle bokeh accents in mint and pink, soft vignette, clean and minimal, no text, no characters, exact 1080x1920px, opaque background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_close_red_normal.png (160x170)
- template: BTN_CLOSE

**Positive prompt**
~~~
close button background, orange-red candy plastic, rounded square, thick outline, top-left highlight, soft inner shadow, Normal state, no text (X glyph is separate icon layer if needed), exact 160x170px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_close_red_pressed.png (160x170)
- template: BTN_CLOSE

**Positive prompt**
~~~
close button background, orange-red candy plastic, rounded square, thick outline, top-left highlight, soft inner shadow, Pressed state, no text (X glyph is separate icon layer if needed), exact 160x170px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_price_green_disabled.png (384x198)
- template: BTN_PRICE
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
price button background, green candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, Disabled state, no text, exact 384x198px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_price_green_normal.png (384x198)
- template: BTN_PRICE
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
price button background, green candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, Normal state, no text, exact 384x198px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_price_green_pressed.png (384x194)
- template: BTN_PRICE
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
price button background, green candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, Pressed state, no text, exact 384x194px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_blue_disabled.png (464x228)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Blue candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state, no text, exact 464x228px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_blue_normal.png (464x228)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Blue candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state, no text, exact 464x228px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_blue_pressed.png (464x224)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Blue candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state, no text, exact 464x224px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_green_disabled.png (464x228)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Green candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state, no text, exact 464x228px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_green_normal.png (464x228)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Green candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state, no text, exact 464x228px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_green_pressed.png (464x224)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Green candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state, no text, exact 464x224px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_orange_disabled.png (464x228)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state, no text, exact 464x228px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_orange_normal.png (464x228)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state, no text, exact 464x228px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_orange_pressed.png (464x224)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state, no text, exact 464x224px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_red_disabled.png (464x228)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Red candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state, no text, exact 464x228px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_red_normal.png (464x228)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Red candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state, no text, exact 464x228px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_red_pressed.png (464x224)
- template: BTN_SMALL
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Red candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state, no text, exact 464x224px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/card_setting_row.png (864x248)
- template: CARD_ROW
- nine-slice border: 70,70,50,50

**Positive prompt**
~~~
settings row card background, creamy plastic, rounded rectangle, subtle bevel, soft inner shadow, thin outline, no text, exact 864x248px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_0.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '0.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_1.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '1.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_2.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '2.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_3.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '3.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_4.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '4.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_5.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '5.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_6.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '6.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_7.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '7.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_8.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '8.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_9.png (64x88)
- template: DIGIT

**Positive prompt**
~~~
single digit glyph '9.png', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 64x88px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/heart_big.png (352x362)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of heart, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 352x362px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/hud_level_label_bg.png (392x138)
- template: HUD_LABEL

**Positive prompt**
~~~
HUD label background, creamy fill, subtle bevel, thin outline, soft inner shadow, no text, exact 392x138px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/hud_pill_dark.png (452x138)
- template: HUD_PILL
- nine-slice border: 40,40,30,30

**Positive prompt**
~~~
HUD pill background, dark chocolate or dark navy fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, soft highlight, no text, exact 452x138px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/hud_pill_dark_small.png (352x126)
- template: HUD_PILL
- nine-slice border: 40,40,30,30

**Positive prompt**
~~~
HUD pill background, dark chocolate or dark navy fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, soft highlight, no text, exact 352x126px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/hud_pill_dark_tiny.png (292x126)
- template: HUD_PILL
- nine-slice border: 40,40,30,30

**Positive prompt**
~~~
HUD pill background, dark chocolate or dark navy fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, soft highlight, no text, exact 292x126px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_clock.png (224x234)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of clock.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 224x234px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_clock_128.png (160x170)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of clock 128.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 160x170px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_close.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of close.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin.png (224x234)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of coin.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 224x234px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_128.png (160x170)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of coin 128.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 160x170px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_bag.png (296x308)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of coin bag.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 296x308px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_chest.png (296x308)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of coin chest.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 296x308px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_safe.png (296x308)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of coin safe.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 296x308px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_stack.png (292x302)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of coin stack.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 292x302px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_fill.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of fill.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_gear.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of gear.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_heart.png (224x234)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of heart.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 224x234px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_heart_128.png (160x170)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of heart 128.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 160x170px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_lock.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of lock.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_loop.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of loop.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_music.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of music.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_next.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of next.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_no_ads_tv.png (296x310)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of no ads tv.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 296x310px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_pause.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of pause.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_plus.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of plus.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_retry.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of retry.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_shop.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of shop.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_shuffle.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of shuffle.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_shuffle_noframe.png.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of shuffle noframe.png.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_sort_noframe.png.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of sort noframe.png.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_vibrate.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of vibrate.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_video.png (192x192)
- template: ICON_GLYPH

**Positive prompt**
~~~
UI icon glyph of video.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/lock_chip_plate.png (292x132)
- template: LOCK_CHIP_PLATE
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
lock chip plate background, rounded rectangle, creamy plastic, subtle bevel, medium outline, soft inner shadow, no text, exact 292x132px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/lock_node_base.png (360x374)
- template: LOCK_NODE_BASE

**Positive prompt**
~~~
lock node base plate, rounded square with thick soft frame, creamy plastic, subtle bevel, inner shadow, no text, exact 360x374px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/lock_node_label_bg.png (252x112)
- template: LOCK_NODE_LABEL_BG
- nine-slice border: 30,30,20,20

**Positive prompt**
~~~
lock node label background, small rounded pill, creamy plastic, subtle bevel, thin outline, soft inner shadow, no text, exact 252x112px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/lock_node_lock.png (212x222)
- template: LOCK_NODE_LOCK

**Positive prompt**
~~~
UI lock icon glyph, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 212x222px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_long_disabled.png (944x318)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Mint candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_long_normal.png (944x318)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Mint candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_long_pressed.png (944x314)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Mint candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x314px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_square_disabled.png (552x566)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Mint candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_square_normal.png (552x566)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Mint candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_square_pressed.png (544x554)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Mint candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 544x554px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_long_disabled.png (944x318)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_long_normal.png (944x318)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_long_pressed.png (944x314)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x314px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_square_disabled.png (552x566)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Orange candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_square_normal.png (552x566)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Orange candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_square_pressed.png (544x554)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Orange candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 544x554px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/overlay_dim.png (1080x1920)
- template: OVERLAY_DIM

**Positive prompt**
~~~
full-screen dim overlay for mobile UI, smooth dark gradient, subtle noise, no hard edges, no text, exact 1080x1920px, opaque background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/panel_modal.png (916x794)
- template: PANEL_BASE
- nine-slice border: 120,120,120,120

**Positive prompt**
~~~
UI panel background, rounded rectangle, cream or blue fill with soft inner gradient, gold or white thick frame, beveled edges, inner shadow, gentle highlight, no text, exact 916x794px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/panel_result.png (956x794)
- template: PANEL_BASE
- nine-slice border: 120,120,120,120

**Positive prompt**
~~~
UI panel background, rounded rectangle, cream or blue fill with soft inner gradient, gold or white thick frame, beveled edges, inner shadow, gentle highlight, no text, exact 956x794px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/panel_thick_gold_blue.png (960x1140)
- template: PANEL_BASE
- nine-slice border: 140,140,140,140

**Positive prompt**
~~~
UI panel background, rounded rectangle, blue fill with soft inner gradient, gold thick frame, beveled edges, inner shadow, gentle highlight, no text, exact 960x1140px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pill_bg.png (392x162)
- template: PILL_BG
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
long pill button background, warm beige cream candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 392x162px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pill_bg_disabled.png (392x162)
- template: PILL_BG
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
long pill button background, warm beige cream candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 392x162px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pill_bg_pressed.png (392x162)
- template: PILL_BG
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
long pill button background, warm beige cream candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 392x162px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pill_timer_beige.png (464x178)
- template: PILL_TIMER

**Positive prompt**
~~~
timer pill background, warm beige cream candy plastic, rounded capsule, subtle bevel, thin outline, soft inner shadow, no text, exact 464x178px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_long_disabled.png (944x318)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Pink candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_long_normal.png (944x318)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Pink candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_long_pressed.png (944x314)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Pink candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x314px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_square_disabled.png (552x566)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Pink candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_square_normal.png (552x566)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Pink candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_square_pressed.png (544x554)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Pink candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 544x554px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_long_disabled.png (944x318)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Purple candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_long_normal.png (944x318)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Purple candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_long_pressed.png (944x314)
- template: BTN_LONG
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Purple candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x314px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_square_disabled.png (552x566)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Purple candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_square_normal.png (552x566)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Purple candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_square_pressed.png (544x554)
- template: BTN_SQUARE
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Purple candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 544x554px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_card_beige.png (1048x324)
- template: SHOP_CARD
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop item card background, wide rounded rectangle, warm beige cream fill, subtle inner gradient, medium outline, soft bevel, gentle inner shadow, faint highlight at top-left, no text, no icons, exact 1048x324px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_card_purple.png (1048x324)
- template: SHOP_CARD
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop item card background, wide rounded rectangle, soft lavender purple fill, subtle inner gradient, medium outline, soft bevel, gentle inner shadow, faint highlight at top-left, no text, no icons, exact 1048x324px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_card_yellow.png (1048x324)
- template: SHOP_CARD
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop item card background, wide rounded rectangle, soft butter yellow fill, subtle inner gradient, medium outline, soft bevel, gentle inner shadow, faint highlight at top-left, no text, no icons, exact 1048x324px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_group_bar.png (752x138)
- template: SHOP_GROUP_BAR
- nine-slice border: 48,48,30,30

**Positive prompt**
~~~
shop group header bar background, rounded pill, dark chocolate fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, no text, exact 752x138px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_row_beige.png (1044x258)
- template: SHOP_ROW
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop row background, wide rounded rectangle, warm beige cream fill, subtle inner gradient, thin outline, soft bevel, gentle inner shadow, no text, no icons, exact 1044x258px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_row_purple.png (1044x258)
- template: SHOP_ROW
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop row background, wide rounded rectangle, soft lavender purple fill, subtle inner gradient, thin outline, soft bevel, gentle inner shadow, no text, no icons, exact 1044x258px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_row_yellow.png (1044x258)
- template: SHOP_ROW
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop row background, wide rounded rectangle, soft butter yellow fill, subtle inner gradient, thin outline, soft bevel, gentle inner shadow, no text, no icons, exact 1044x258px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_scroll_fade_bottom.png (1080x180)
- template: SCROLL_FADE

**Positive prompt**
~~~
scroll view edge fade overlay, bottom fade, smooth alpha gradient from transparent to semi-opaque cream tint, no hard edges, no text, exact 1080x180px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_scroll_fade_top.png (1080x180)
- template: SCROLL_FADE

**Positive prompt**
~~~
scroll view edge fade overlay, top fade, smooth alpha gradient from transparent to semi-opaque cream tint, no hard edges, no text, exact 1080x180px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_topbar_scallop_tile_512x128.png (512x128)
- template: SCALLOP_TILE

**Positive prompt**
~~~
seamless scallop decorative trim tile, horizontally tileable, creamy plastic material, soft inner shadow, subtle highlight, no text, no icons, no background, exact 512x128px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/tag_fast_danger_bg.png (362x120)
- template: TAG_PILL
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
pill tag background, danger (orange-red) mood, rounded capsule, thin outline, subtle highlight, soft inner shadow, no text, exact 362x120px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/tag_fast_info_bg.png (362x120)
- template: TAG_PILL
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
pill tag background, info (mint/blue) mood, rounded capsule, thin outline, subtle highlight, soft inner shadow, no text, exact 362x120px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/tag_small_info_bg.png (292x112)
- template: TAG_PILL_SMALL
- nine-slice border: 50,50,30,30

**Positive prompt**
~~~
small pill tag background, info mood, rounded capsule, thin outline, subtle highlight, soft inner shadow, no text, exact 292x112px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_full_off.png (292x128)
- template: TOGGLE

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 292x128px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_full_on.png (292x128)
- template: TOGGLE

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 292x128px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_knob.png (88x96)
- template: TOGGLE

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 88x96px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_track_off.png (220x60)
- template: TOGGLE

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 220x60px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_track_on.png (220x60)
- template: TOGGLE

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 220x60px, transparent background, centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~


