#version 330 core
layout(location = 0) in vec2 position;
layout(location = 1) in vec2 texcoordv;
out vec2 texcoord;

void main()
{
	texcoord = texcoordv;
	gl_Position = vec4(position, 0.0, 1.0);
}
