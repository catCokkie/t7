shader_type canvas_item;

uniform vec2 center = vec2(0.5, 0.5);
uniform float radius = 0.75;
uniform float softness = 0.45;
uniform float intensity = 0.0; // 0..1
uniform vec4 color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

void fragment() {
    vec2 uv = SCREEN_UV;
    float dist = distance(uv, center);
    float vig = smoothstep(radius, radius - softness, dist);
    vig = clamp(vig * intensity, 0.0, 1.0);
    vec4 base = texture(SCREEN_TEXTURE, SCREEN_UV);
    // Darken edges by mixing toward color
    COLOR = mix(base, color, vig);
}

