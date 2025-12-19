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
centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG
~~~

---

**Note (Consolidated):**
- This is the single canonical prompt sheet for v0.5. Other `_prompt_sheet_*_v05.md` files have been removed.
- Sections are grouped by **TAG** (directory name).
- Add new items here; keep the `## Dir/filename.png (WxH)` header format.

---

### TAG: UI_Sprites

## UI_Sprites/badge_red_bg.png (152x162)
- template: BADGE_BG
- background: transparent

**Positive prompt**
~~~
small notification badge background, red candy plastic, round shape, thick outline, glossy highlight, soft inner shadow, no text, exact 152x162px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/bg_main.png (1080x1920)
- template: BG_MAIN
- background: opaque

**Positive prompt**
~~~
warm creamy background gradient, subtle bokeh accents in mint and pink, soft vignette, clean and minimal, no text, no characters, exact 1080x1920px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_close_red_normal.png (160x170)
- template: BTN_CLOSE
- background: transparent

**Positive prompt**
~~~
close button background, orange-red candy plastic, rounded square, thick outline, top-left highlight, soft inner shadow, Normal state, no text (X glyph is separate icon layer if needed), exact 160x170px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_close_red_pressed.png (160x170)
- template: BTN_CLOSE
- background: transparent

**Positive prompt**
~~~
close button background, orange-red candy plastic, rounded square, thick outline, top-left highlight, soft inner shadow, Pressed state, no text (X glyph is separate icon layer if needed), exact 160x170px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_price_green_disabled.png (384x198)
- template: BTN_PRICE
- background: transparent
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
price button background, green candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, Disabled state, no text, exact 384x198px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_price_green_normal.png (384x198)
- template: BTN_PRICE
- background: transparent
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
price button background, green candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, Normal state, no text, exact 384x198px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_price_green_pressed.png (384x194)
- template: BTN_PRICE
- background: transparent
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
price button background, green candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, Pressed state, no text, exact 384x194px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_blue_disabled.png (464x228)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Blue candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state, no text, exact 464x228px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_blue_normal.png (464x228)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Blue candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state, no text, exact 464x228px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_blue_pressed.png (464x224)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Blue candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state, no text, exact 464x224px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_green_disabled.png (464x228)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Green candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state, no text, exact 464x228px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_green_normal.png (464x228)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Green candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state, no text, exact 464x228px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_green_pressed.png (464x224)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Green candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state, no text, exact 464x224px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_orange_disabled.png (464x228)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state, no text, exact 464x228px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_orange_normal.png (464x228)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state, no text, exact 464x228px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_orange_pressed.png (464x224)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state, no text, exact 464x224px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_red_disabled.png (464x228)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Red candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state, no text, exact 464x228px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_red_normal.png (464x228)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Red candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state, no text, exact 464x228px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/btn_small_red_pressed.png (464x224)
- template: BTN_SMALL
- background: transparent
- nine-slice border: 80,80,55,55

**Positive prompt**
~~~
small rounded button base, Red candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state, no text, exact 464x224px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/card_setting_row.png (864x248)
- template: CARD_ROW
- background: transparent
- nine-slice border: 70,70,50,50

**Positive prompt**
~~~
settings row card background, creamy plastic, rounded rectangle, subtle bevel, soft inner shadow, thin outline, no text, exact 864x248px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_0.png (1024x1024)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '0', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_1.png (1024x1024)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '1', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_2.png (1024x1024)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '2', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_3.png (1024x1536)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '3', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1536px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_4.png (1024x1024)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '4', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_5.png (1024x1536)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '5', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1536px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_6.png (1024x1024)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '6', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_7.png (1024x1536)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '7', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1536px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_8.png (1024x1536)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '8', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1536px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/digit_9.png (1024x1536)
- template: DIGIT
- background: transparent

**Positive prompt**
~~~
single digit glyph '9', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background, exact 1024x1536px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/heart_big.png (352x362)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of heart, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 352x362px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/hud_level_label_bg.png (392x138)
- template: HUD_LABEL
- background: transparent

**Positive prompt**
~~~
HUD label background, creamy fill, subtle bevel, thin outline, soft inner shadow, no text, exact 392x138px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/hud_pill_dark.png (452x138)
- template: HUD_PILL
- background: transparent
- nine-slice border: 40,40,30,30

**Positive prompt**
~~~
HUD pill background, dark chocolate or dark navy fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, soft highlight, no text, exact 452x138px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/hud_pill_dark_small.png (352x126)
- template: HUD_PILL
- background: transparent
- nine-slice border: 40,40,30,30

**Positive prompt**
~~~
HUD pill background, dark chocolate or dark navy fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, soft highlight, no text, exact 352x126px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/hud_pill_dark_tiny.png (292x126)
- template: HUD_PILL
- background: transparent
- nine-slice border: 40,40,30,30

**Positive prompt**
~~~
HUD pill background, dark chocolate or dark navy fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, soft highlight, no text, exact 292x126px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_clock.png (224x234)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of clock, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 224x234px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_clock_128.png (160x170)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of clock, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 160x170px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_close.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of close, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin.png (224x234)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of coin, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 224x234px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_128.png (160x170)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of coin, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 160x170px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_bag.png (296x308)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of coin bag, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 296x308px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_chest.png (296x308)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of coin chest, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 296x308px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_safe.png (296x308)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of coin safe, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 296x308px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_coin_stack.png (292x302)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of coin stack, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 292x302px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_fill.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of fill, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_gear.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of gear, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_heart.png (224x234)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of heart, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 224x234px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_heart_128.png (160x170)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of heart, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 160x170px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_lock.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of lock, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_loop.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of loop, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_music.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of music, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_next.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of next, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_no_ads_tv.png (296x310)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of no ads tv, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 296x310px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_pause.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of pause, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_plus.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of plus, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_retry.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of retry, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_shop.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of shop, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_shuffle.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of shuffle, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_shuffle_noframe.png.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of shuffle noframe.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_sort_noframe.png.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of sort noframe.png, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_vibrate.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of vibrate, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/icon_video.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of video, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/lock_chip_plate.png (292x132)
- template: LOCK_CHIP_PLATE
- background: transparent
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
lock chip plate background, rounded rectangle, creamy plastic, subtle bevel, medium outline, soft inner shadow, no text, exact 292x132px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/lock_node_base.png (360x374)
- template: LOCK_NODE_BASE
- background: transparent

**Positive prompt**
~~~
lock node base plate, rounded square with thick soft frame, creamy plastic, subtle bevel, inner shadow, no text, exact 360x374px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/lock_node_label_bg.png (252x112)
- template: LOCK_NODE_LABEL_BG
- background: transparent
- nine-slice border: 30,30,20,20

**Positive prompt**
~~~
lock node label background, small rounded pill, creamy plastic, subtle bevel, thin outline, soft inner shadow, no text, exact 252x112px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/lock_node_lock.png (212x222)
- template: LOCK_NODE_LOCK
- background: transparent

**Positive prompt**
~~~
UI lock icon glyph, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 212x222px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_long_disabled.png (944x318)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Mint candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_long_normal.png (944x318)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Mint candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_long_pressed.png (944x314)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Mint candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x314px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_square_disabled.png (552x566)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Mint candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_square_normal.png (552x566)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Mint candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/mint_square_pressed.png (544x554)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Mint candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 544x554px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_long_disabled.png (944x318)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_long_normal.png (944x318)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_long_pressed.png (944x314)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Orange candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x314px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_square_disabled.png (552x566)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Orange candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_square_normal.png (552x566)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Orange candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/orange_square_pressed.png (544x554)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Orange candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 544x554px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/overlay_dim.png (1080x1920)
- template: OVERLAY_DIM
- background: opaque

**Positive prompt**
~~~
full-screen dim overlay for mobile UI, smooth dark gradient, subtle noise, no hard edges, no text, exact 1080x1920px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/panel_modal.png (916x794)
- template: PANEL_BASE
- background: transparent
- nine-slice border: 120,120,120,120

**Positive prompt**
~~~
UI panel background, rounded rectangle, cream or blue fill with soft inner gradient, gold or white thick frame, beveled edges, inner shadow, gentle highlight, no text, exact 916x794px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/panel_result.png (956x794)
- template: PANEL_BASE
- background: transparent
- nine-slice border: 120,120,120,120

**Positive prompt**
~~~
UI panel background, rounded rectangle, cream or blue fill with soft inner gradient, gold or white thick frame, beveled edges, inner shadow, gentle highlight, no text, exact 956x794px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/panel_thick_gold_blue.png (960x1140)
- template: PANEL_BASE
- background: transparent
- nine-slice border: 140,140,140,140

**Positive prompt**
~~~
UI panel background, rounded rectangle, blue fill with soft inner gradient, gold thick frame, beveled edges, inner shadow, gentle highlight, no text, exact 960x1140px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pill_bg.png (392x162)
- template: PILL_BG
- background: transparent
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
long pill button background, warm beige cream candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 392x162px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pill_bg_disabled.png (392x162)
- template: PILL_BG
- background: transparent
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
long pill button background, warm beige cream candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 392x162px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pill_bg_pressed.png (392x162)
- template: PILL_BG
- background: transparent
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
long pill button background, warm beige cream candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 392x162px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pill_timer_beige.png (464x178)
- template: PILL_TIMER
- background: transparent

**Positive prompt**
~~~
timer pill background, warm beige cream candy plastic, rounded capsule, subtle bevel, thin outline, soft inner shadow, no text, exact 464x178px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_long_disabled.png (944x318)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Pink candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_long_normal.png (944x318)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Pink candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_long_pressed.png (944x314)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Pink candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x314px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_square_disabled.png (552x566)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Pink candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_square_normal.png (552x566)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Pink candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/pink_square_pressed.png (544x554)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Pink candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 544x554px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_long_disabled.png (944x318)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Purple candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_long_normal.png (944x318)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Purple candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x318px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_long_pressed.png (944x314)
- template: BTN_LONG
- background: transparent
- nine-slice border: 140,140,90,90

**Positive prompt**
~~~
long pill button base, Purple candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 944x314px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_square_disabled.png (552x566)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Purple candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Disabled state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_square_normal.png (552x566)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Purple candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Normal state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 552x566px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/purple_square_pressed.png (544x554)
- template: BTN_SQUARE
- background: transparent
- nine-slice border: 170,170,170,170

**Positive prompt**
~~~
square rounded button base, Purple candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, Pressed state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text, exact 544x554px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_card_beige.png (1048x324)
- template: SHOP_CARD
- background: transparent
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop item card background, wide rounded rectangle, warm beige cream fill, subtle inner gradient, medium outline, soft bevel, gentle inner shadow, faint highlight at top-left, no text, no icons, exact 1048x324px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_card_purple.png (1048x324)
- template: SHOP_CARD
- background: transparent
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop item card background, wide rounded rectangle, soft lavender purple fill, subtle inner gradient, medium outline, soft bevel, gentle inner shadow, faint highlight at top-left, no text, no icons, exact 1048x324px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_card_yellow.png (1048x324)
- template: SHOP_CARD
- background: transparent
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop item card background, wide rounded rectangle, soft butter yellow fill, subtle inner gradient, medium outline, soft bevel, gentle inner shadow, faint highlight at top-left, no text, no icons, exact 1048x324px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_group_bar.png (752x138)
- template: SHOP_GROUP_BAR
- background: transparent
- nine-slice border: 48,48,30,30

**Positive prompt**
~~~
shop group header bar background, rounded pill, dark chocolate fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, no text, exact 752x138px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_row_beige.png (1044x258)
- template: SHOP_ROW
- background: transparent
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop row background, wide rounded rectangle, warm beige cream fill, subtle inner gradient, thin outline, soft bevel, gentle inner shadow, no text, no icons, exact 1044x258px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_row_purple.png (1536x1024)
- template: SHOP_ROW
- background: transparent
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop row background, wide rounded rectangle, soft lavender purple fill, subtle inner gradient, thin outline, soft bevel, gentle inner shadow, no text, no icons, exact 1536x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_row_yellow.png (1044x258)
- template: SHOP_ROW
- background: transparent
- nine-slice border: 90,90,60,60

**Positive prompt**
~~~
shop row background, wide rounded rectangle, soft butter yellow fill, subtle inner gradient, thin outline, soft bevel, gentle inner shadow, no text, no icons, exact 1044x258px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_scroll_fade_bottom.png (1080x180)
- template: SCROLL_FADE
- background: transparent

**Positive prompt**
~~~
scroll view edge fade overlay, bottom fade, smooth alpha gradient from transparent to semi-opaque cream tint, no hard edges, no text, exact 1080x180px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_scroll_fade_top.png (1080x180)
- template: SCROLL_FADE
- background: transparent

**Positive prompt**
~~~
scroll view edge fade overlay, top fade, smooth alpha gradient from transparent to semi-opaque cream tint, no hard edges, no text, exact 1080x180px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/shop_topbar_scallop_tile_512x128.png (1536x1024)
- template: SCALLOP_TILE
- background: transparent

**Positive prompt**
~~~
seamless scallop decorative trim tile, horizontally tileable, creamy plastic material, soft inner shadow, subtle highlight, no text, no icons, no background, exact 1536x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/tag_fast_danger_bg.png (362x120)
- template: TAG_PILL
- background: transparent
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
pill tag background, danger (orange-red) mood, rounded capsule, thin outline, subtle highlight, soft inner shadow, no text, exact 362x120px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/tag_fast_info_bg.png (362x120)
- template: TAG_PILL
- background: transparent
- nine-slice border: 60,60,40,40

**Positive prompt**
~~~
pill tag background, info (mint/blue) mood, rounded capsule, thin outline, subtle highlight, soft inner shadow, no text, exact 362x120px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/tag_small_info_bg.png (292x112)
- template: TAG_PILL_SMALL
- background: transparent
- nine-slice border: 50,50,30,30

**Positive prompt**
~~~
small pill tag background, info mood, rounded capsule, thin outline, subtle highlight, soft inner shadow, no text, exact 292x112px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_full_off.png (292x128)
- template: TOGGLE
- background: transparent

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 292x128px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_full_on.png (292x128)
- template: TOGGLE
- background: transparent

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 292x128px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_knob.png (1024x1024)
- template: TOGGLE
- background: transparent

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_track_off.png (1536x1024)
- template: TOGGLE
- background: transparent

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 1536x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## UI_Sprites/toggle_track_on.png (1536x1024)
- template: TOGGLE
- background: transparent

**Positive prompt**
~~~
toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text, exact 1536x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

### TAG: World_Sprites

## World_Sprites/box_completed_badge_check_256.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_completed_badge_check_512.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_completed_frame_glow_1024.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_completed_frame_glow_512.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_completed_glass_overlay_1024.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_completed_glass_overlay_512.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_outline_dashed_open_right.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
3D creamy plastic toy storage box rim frame for holding lego-like bricks, rounded-square outer frame with visible wall thickness, soft bevel, subtle top-left highlight, gentle ambient occlusion, soft inner shadow along the inside edge, hollow CENTER cutout is fully transparent (alpha 0), NO background fill, OPENING GAP on the RIGHT edge (missing wall segment), clean smooth opening, 9-slice friendly: uniform rim thickness and smooth corners, no unique corner decorations, no repeating patterns, no dashed lines, no text, exact 1024x1024px, centered, no cropping, generous safe padding (~10% of canvas), all shadows fully inside frame, crisp edges, sRGB PNG, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, hyper-real photo, realistic scene, isometric, perspective skew, 3D scene, background scene, opaque background, gradient background, checkerboard background, alpha grid, transparency grid, dashed, dotted, outline-only stroke, flat 2D line art, wood texture, metal texture, fabric, harsh reflections, excessive noise, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_outline_dashed_open_top.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
3D creamy plastic toy storage box rim frame for holding lego-like bricks, rounded-square outer frame with visible wall thickness, soft bevel, subtle top-left highlight, gentle ambient occlusion, soft inner shadow along the inside edge, hollow CENTER cutout is fully transparent (alpha 0), NO background fill, OPENING GAP on the TOP edge (missing wall segment), clean smooth opening, 9-slice friendly: uniform rim thickness and smooth corners, no unique corner decorations, no repeating patterns, no dashed lines, no text, exact 1024x1024px, centered, no cropping, generous safe padding (~10% of canvas), all shadows fully inside frame, crisp edges, sRGB PNG, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, hyper-real photo, realistic scene, isometric, perspective skew, 3D scene, background scene, opaque background, gradient background, checkerboard background, alpha grid, transparency grid, dashed, dotted, outline-only stroke, flat 2D line art, wood texture, metal texture, fabric, harsh reflections, excessive noise, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_outline_dashed_open_bottom.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
3D creamy plastic toy storage box rim frame for holding lego-like bricks, rounded-square outer frame with visible wall thickness, soft bevel, subtle top-left highlight, gentle ambient occlusion, soft inner shadow along the inside edge, hollow CENTER cutout is fully transparent (alpha 0), NO background fill, OPENING GAP on the BOTTOM edge (missing wall segment), clean smooth opening, 9-slice friendly: uniform rim thickness and smooth corners, no unique corner decorations, no repeating patterns, no dashed lines, no text, exact 1024x1024px, centered, no cropping, generous safe padding (~10% of canvas), all shadows fully inside frame, crisp edges, sRGB PNG, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, hyper-real photo, realistic scene, isometric, perspective skew, 3D scene, background scene, opaque background, gradient background, checkerboard background, alpha grid, transparency grid, dashed, dotted, outline-only stroke, flat 2D line art, wood texture, metal texture, fabric, harsh reflections, excessive noise, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_outline_dashed_open_left.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
3D creamy plastic toy storage box rim frame for holding lego-like bricks, rounded-square outer frame with visible wall thickness, soft bevel, subtle top-left highlight, gentle ambient occlusion, soft inner shadow along the inside edge, hollow CENTER cutout is fully transparent (alpha 0), NO background fill, OPENING GAP on the LEFT edge (missing wall segment), clean smooth opening, 9-slice friendly: uniform rim thickness and smooth corners, no unique corner decorations, no repeating patterns, no dashed lines, no text, exact 1024x1024px, centered, no cropping, generous safe padding (~10% of canvas), all shadows fully inside frame, crisp edges, sRGB PNG, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, hyper-real photo, realistic scene, isometric, perspective skew, 3D scene, background scene, opaque background, gradient background, checkerboard background, alpha grid, transparency grid, dashed, dotted, outline-only stroke, flat 2D line art, wood texture, metal texture, fabric, harsh reflections, excessive noise, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_cavity_open_top.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
inner cavity floor insert for a toy storage box (open-top), subtle recessed cavity shading that sits BEHIND the blocks: soft inner shadow around the inside edge, gentle ambient occlusion, faint top-left highlight, smooth creamy plastic feel, CENTER area lightly darkened but not opaque, OUTSIDE the cavity is fully transparent (alpha 0), no rim frame, no thick outline, OPENING GAP on the TOP edge (recess shadow breaks/open), 9-slice friendly: uniform border region, no unique corner details, no repeating patterns, no dashed lines, no text, exact 1024x1024px, centered, no cropping, generous safe padding (~10%), all shading inside frame, crisp edges, sRGB PNG, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, hyper-real photo, realistic scene, isometric, perspective skew, 3D scene, background scene, opaque background, gradient background, checkerboard background, alpha grid, transparency grid, thick outer rim frame, stroke-only outline, dashed, dotted, wood texture, metal texture, fabric, harsh reflections, excessive noise, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_cavity_open_right.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
inner cavity floor insert for a toy storage box (open-right), subtle recessed cavity shading that sits BEHIND the blocks: soft inner shadow around the inside edge, gentle ambient occlusion, faint top-left highlight, smooth creamy plastic feel, CENTER area lightly darkened but not opaque, OUTSIDE the cavity is fully transparent (alpha 0), no rim frame, no thick outline, OPENING GAP on the RIGHT edge (recess shadow breaks/open), 9-slice friendly: uniform border region, no unique corner details, no repeating patterns, no dashed lines, no text, exact 1024x1024px, centered, no cropping, generous safe padding (~10%), all shading inside frame, crisp edges, sRGB PNG, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, hyper-real photo, realistic scene, isometric, perspective skew, 3D scene, background scene, opaque background, gradient background, checkerboard background, alpha grid, transparency grid, thick outer rim frame, stroke-only outline, dashed, dotted, wood texture, metal texture, fabric, harsh reflections, excessive noise, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_cavity_open_bottom.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
inner cavity floor insert for a toy storage box (open-bottom), subtle recessed cavity shading that sits BEHIND the blocks: soft inner shadow around the inside edge, gentle ambient occlusion, faint top-left highlight, smooth creamy plastic feel, CENTER area lightly darkened but not opaque, OUTSIDE the cavity is fully transparent (alpha 0), no rim frame, no thick outline, OPENING GAP on the BOTTOM edge (recess shadow breaks/open), 9-slice friendly: uniform border region, no unique corner details, no repeating patterns, no dashed lines, no text, exact 1024x1024px, centered, no cropping, generous safe padding (~10%), all shading inside frame, crisp edges, sRGB PNG, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, hyper-real photo, realistic scene, isometric, perspective skew, 3D scene, background scene, opaque background, gradient background, checkerboard background, alpha grid, transparency grid, thick outer rim frame, stroke-only outline, dashed, dotted, wood texture, metal texture, fabric, harsh reflections, excessive noise, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/box_cavity_open_left.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
inner cavity floor insert for a toy storage box (open-left), subtle recessed cavity shading that sits BEHIND the blocks: soft inner shadow around the inside edge, gentle ambient occlusion, faint top-left highlight, smooth creamy plastic feel, CENTER area lightly darkened but not opaque, OUTSIDE the cavity is fully transparent (alpha 0), no rim frame, no thick outline, OPENING GAP on the LEFT edge (recess shadow breaks/open), 9-slice friendly: uniform border region, no unique corner details, no repeating patterns, no dashed lines, no text, exact 1024x1024px, centered, no cropping, generous safe padding (~10%), all shading inside frame, crisp edges, sRGB PNG, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, hyper-real photo, realistic scene, isometric, perspective skew, 3D scene, background scene, opaque background, gradient background, checkerboard background, alpha grid, transparency grid, thick outer rim frame, stroke-only outline, dashed, dotted, wood texture, metal texture, fabric, harsh reflections, excessive noise, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/completed_overlay.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/conveyor_slot.png (1536x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1536x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/lock_marker_color_disc.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/lock_marker_lock_icon.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/lock_marker_plate.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/lock_overlay.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/run_outline_9slice.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_complete_burst_sheet_8f_1024x512.png (1536x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1536x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_complete_burst_sheet_8f_512x256.png (1536x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1536x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_confetti_rect_128.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_confetti_rect_256.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_confetti_star_128.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_confetti_star_256.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_confetti_stream_128.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_confetti_stream_256.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_confetti_tri_128.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_confetti_tri_256.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_firework_confetti_burst_sheet_16f_1024x1024.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_firework_confetti_burst_sheet_16f_2048x2048.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_sparkle_star_128.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## World_Sprites/vfx_sparkle_star_256.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

### TAG: conveyor_belt_texture_v02_candy

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_mint_256x64.png (256x64)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 256x64px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_mint_256x64_alphaEdges.png (256x64)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 256x64px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_mint_512x128.png (512x128)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 512x128px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_mint_512x128_alphaEdges.png (512x128)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 512x128px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_orange_256x64.png (256x64)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 256x64px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_orange_256x64_alphaEdges.png (256x64)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 256x64px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_orange_512x128.png (512x128)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 512x128px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_orange_512x128_alphaEdges.png (512x128)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 512x128px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_purple_256x64.png (256x64)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 256x64px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_purple_256x64_alphaEdges.png (256x64)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 256x64px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_purple_512x128.png (512x128)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 512x128px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/conveyor_belt_candy_purple_512x128_alphaEdges.png (512x128)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 512x128px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/icon_shuffle_noframe.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of shuffle noframe, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/icon_sort.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of sort, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/icon_sort_noframe.png (192x192)
- template: ICON_GLYPH
- background: transparent

**Positive prompt**
~~~
UI icon glyph of sort noframe, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text, exact 192x192px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/preview_sheet_all_colorways.png (1600x891)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1600x891px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/preview_tile_mint_512x128_3x2.png (1556x266)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1556x266px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/preview_tile_orange_512x128_3x2.png (1556x266)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1556x266px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## conveyor_belt_texture_v02_candy/preview_tile_purple_512x128_3x2.png (1556x266)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1556x266px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

### TAG: setting_page_assets

## setting_page_assets/btn_close.png (143x144)

**Positive prompt**
~~~
Close button (Settings overlay), **full button including the close glyph**:
- Shape: small rounded-square button, slightly taller than wide (143x144), thick outer rim/frame, slightly recessed inner face (concave), soft bevel.
- Material: creamy plastic candy style, warm orange / orange-red, top-left highlight, soft inner shadow, subtle ambient occlusion.
- Center glyph: a clean bold **X** close icon, warm white fill, thick dark brown outline, tiny micro-shadow to separate from the face; centered and upright; no circle/plate behind it.
- Normal state: brighter highlight and a slightly longer drop shadow.
Transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, orthographic front view, no extra UI elements.
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## setting_page_assets/btn_close_pressed.png (143x144)

**Positive prompt**
~~~
Close button (Settings overlay) **PRESSED state**, full button including the close glyph:
- Same shape as normal: rounded-square, thick rim, recessed inner face.
- Material: warm orange/orange-red creamy plastic.
- Pressed change: slightly darker and lower contrast, highlight reduced, drop shadow shorter/softer, inner shadow a bit stronger.
- Center glyph: same bold X icon (warm white + dark brown outline), slightly darker and looks pressed in (tiny downward offset or stronger inner shadow).
Transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, orthographic front view, no extra UI elements.
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## setting_page_assets/btn_retry.png (632x244)

**Positive prompt**
~~~
large retry button background, orange candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, normal state, no text, exact 632x244px, transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## setting_page_assets/btn_retry_pressed.png (632x244)

**Positive prompt**
~~~
large retry button background, orange candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, pressed state (slightly darker + reduced shadow), no text, exact 632x244px, transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## setting_page_assets/toggle_off.png (221x132)

**Positive prompt**
~~~
toggle switch OFF state, desaturated dark track, rounded ends, glossy highlight top-left, soft inner shadow, knob on the left, thick outline, no text, exact 221x132px, transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## setting_page_assets/toggle_off_pressed.png (221x132)

**Positive prompt**
~~~
toggle switch OFF pressed state, slightly darker desaturated track, reduced shadow, glossy highlight top-left, knob on the left, thick outline, no text, exact 221x132px, transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## setting_page_assets/toggle_on.png (221x132)

**Positive prompt**
~~~
toggle switch ON state, green candy plastic track with rounded ends, glossy highlight top-left, soft inner shadow, knob on the right, thick outline, no text, exact 221x132px, transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## setting_page_assets/toggle_on_pressed.png (221x132)

**Positive prompt**
~~~
toggle switch ON pressed state, slightly darker green track, reduced shadow, glossy highlight top-left, knob on the right, thick outline, no text, exact 221x132px, transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

### TAG: BoosterPurchase

## BoosterPurchase/btn_buy_coins_80.png (370x235)

**Positive prompt**
~~~
purchase with coins button background, green candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, no text, exact 370x235px, opaque background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## BoosterPurchase/btn_close.png (110x108)

**Positive prompt**
~~~
Close button (BoosterPurchase), **full button including the close glyph**:
- Shape: small rounded-square button (110x108), thick outer rim/frame, slightly recessed inner face (concave), soft bevel.
- Material: creamy plastic candy style, warm orange / orange-red, top-left highlight, soft inner shadow, subtle ambient occlusion.
- Center glyph: a clean bold **X** close icon, warm white fill, thick dark brown outline, tiny micro-shadow to separate from the face; centered and upright; no circle/plate behind it.
- Normal state: brighter highlight and a slightly longer drop shadow.
Transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, orthographic front view, no extra UI elements.
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## BoosterPurchase/btn_watch_ad_free.png (380x235)

**Positive prompt**
~~~
watch ad free button background, orange candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, no text, exact 380x235px, opaque background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## BoosterPurchase/header_title_shuffle.png (880x330)

**Positive prompt**
~~~
wide orange title header background for a popup, rounded rectangle, thick outline, strong top-left highlight, soft inner shadow, no text, exact 880x330px, opaque background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## BoosterPurchase/icon_booster_shuffle.png (424x410)

**Positive prompt**
~~~
booster shuffle icon, crossed tools glyph, mint/teal candy plastic, thick outline, glossy highlight top-left, soft inner shadow, no text, exact 424x410px, transparent background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## BoosterPurchase/icon_coin.png (332x194)

**Positive prompt**
~~~
coin icon for button, gold coin glyph, warm highlights, thick outline, no text, exact 332x194px, transparent background, centered, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## BoosterPurchase/popup_shuffle_full.png (884x1325)

**Positive prompt**
~~~
full popup panel background for booster purchase, creamy beige panel with thick rounded frame, orange title header area at top, soft bevel and inner shadow, no text, no icons, exact 884x1325px, opaque background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## BoosterPurchase/popup_sort_full.png (1024x1024)
- template: GENERIC
- background: transparent

**Positive prompt**
~~~
UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text, exact 1024x1024px, centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

## BoosterPurchase/title_shuffle_text.png (880x330)

**Positive prompt**
~~~
title text sprite for a popup header, word 'SHUFFLE' only, bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, centered, no background, exact 880x330px, transparent background, crisp edges, sRGB, PNG, mobile game UI, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~

### TAG: ResourcesRoot

## ResourcesRoot/setting_page.png (902x1233)

**Positive prompt**
~~~
settings modal panel full background, orange title bar at top, creamy beige body, thick rounded frame, soft inner bevel, consistent top-left highlight, soft ambient occlusion, no extra buttons beyond the design, keep layout clean and centered, exact 902x1233px, opaque background, centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG, soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts
~~~
