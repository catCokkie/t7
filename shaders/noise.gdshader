shader_type canvas_item;

uniform float amount : hint_range(0.0, 1.0, 0.01) = 0.0;
uniform float speed : hint_range(0.0, 4.0, 0.01) = 1.5;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

float random(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898,78.233))) * 43758.5453);
}

void fragment() {
    vec4 base = texture(SCREEN_TEXTURE, SCREEN_UV);
    float n = random(SCREEN_UV * (TIME * speed + 1.0));
    float grain = (n - 0.5) * 2.0 * amount; // -amount..+amount
    vec3 noise_col = vec3(grain);
    COLOR.rgb = base.rgb + noise_col;
    COLOR.a = base.a;
}

