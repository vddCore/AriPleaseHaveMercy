#version 330 core

uniform sampler2D cr_Screen;

in float cr_Time;
in vec2 cr_ScreenSize;

uniform float brightness = 0.08;
uniform bool gauss_horizontal;
uniform float gauss_weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

vec4 effect(vec4 pixel, vec2 tex_coords) {
    vec2 tex_offset = 1.0 / textureSize(cr_Screen, 0);
    vec3 result = texture(cr_Screen, tex_coords).rgb * gauss_weight[0];
    
    if (gauss_horizontal) {
        for (int i = 1; i < 5; i++) {
            result += texture(cr_Screen, tex_coords + vec2(tex_offset.x * i, 0)).rgb * gauss_weight[i];
            result += texture(cr_Screen, tex_coords - vec2(tex_offset.x * i, 0)).rgb * gauss_weight[i];
        }
    } else {
        for (int i = 1; i < 5; i++) {
            result += texture(cr_Screen, tex_coords + vec2(0, tex_offset.y * i)).rgb * gauss_weight[i];
            result += texture(cr_Screen, tex_coords - vec2(0, tex_offset.y * i)).rgb * gauss_weight[i];
        }
    }
    
    return vec4(result + (result * brightness), 1.0);
}