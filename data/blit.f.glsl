#version 330 core
in vec2 texcoord;
layout(location = 0) out vec4 fs_FragColor;
uniform sampler2D source;

void main()
{
	fs_FragColor = texture(source, texcoord);
}
