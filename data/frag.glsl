#version 330 core

// Input.
in vec2 in_FragUV;
in vec2 in_FragExtra;
in vec4 in_FragColor;

// Output to frame buffer.
out vec4 out_FragColor;

// Uniforms.
uniform sampler2D sprite;
uniform sampler2D palette;

// Constants.
const float remaps     = 1.0f / 8.0f;
const float shadow_idx = 4.0f / 255.0f;
const vec4 cloak_color = vec4(0, 0, 0, 0.50);

void main()
{
	float remap = in_FragExtra.x * remaps;
	float cloak = in_FragExtra.y;
	float idx   = texture(sprite, in_FragUV).r;
	vec4 color  = texture(palette, vec2(idx, remap));

	if(idx == shadow_idx)
	{
		color.a *= (1 - cloak);
	}
	else if(idx > 0)
	{
		color = (1 - cloak) * color + cloak * cloak_color;
	}

	out_FragColor = in_FragColor * color;
}