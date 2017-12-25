#version 330 core

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec2 translation;
layout(location = 3) in vec2 scaling;
layout(location = 4) in vec4 region;
layout(location = 5) in vec4 modifiers;
layout(location = 6) in vec4 vchannel;

out vec2 uvcoord;
out vec4 channel;
out float remap;
out float cloak;

uniform mat4 projection;

void main()
{
	channel     = vchannel.bgra;
	remap       = modifiers.x;
	cloak       = modifiers.y;
	uvcoord     = texcoord * region.zw + region.xy;
	gl_Position = projection * vec4(position * scaling + translation, 0.0, 1.0);
}