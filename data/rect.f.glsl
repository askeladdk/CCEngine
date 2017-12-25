#version 330 core

in vec4 color;

layout(location = 0) out vec4 out_FragColor;

void main()
{
	out_FragColor = color;
}