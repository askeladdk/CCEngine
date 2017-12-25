#version 330 core

layout(location = 0) in vec2 position;
layout(location = 1) in mat4 transform;
layout(location = 5) in vec4 vcolor;

out vec4 color;

uniform mat4 projection;

void main()
{
	color = vcolor.bgra;
	gl_Position = projection * transform * vec4(position, 0.0, 1.0);
}