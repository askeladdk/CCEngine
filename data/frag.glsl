#version 330 core

in vec2 uvcoord;
in vec4 channel;
in float remap;
in float cloak;

layout(location = 0) out vec4 out_FragColor;

uniform sampler2D sprite;
uniform sampler2D palette;

// Constants.
const float shadow_idx = 4.0f / 256.0f;
const vec4 cloak_color = vec4(0, 0, 0, 0.50);

void main()
{
	float idx  = texture(sprite, uvcoord).r;
	vec4 color = texture(palette, vec2(idx, remap));

	if(idx == shadow_idx)
		color.a *= (1 - cloak);
	else if(idx > 0)
		color = (1 - cloak) * color + cloak * cloak_color;

	out_FragColor = channel * color;
}