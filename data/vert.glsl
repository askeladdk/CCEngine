#version 330 core

// Input.
layout(location = 0) in vec2 in_Vertex;
layout(location = 1) in vec2 in_UV;
layout(location = 2) in vec4 in_Tile;
layout(location = 3) in vec4 in_FrameUV;
layout(location = 4) in vec2 in_Extra;
layout(location = 5) in vec4 in_Color;

// Output to fragment shader.
out vec2 in_FragUV;
out vec2 in_FragExtra;
out vec4 in_FragColor;

// Uniform variables.
uniform mat4 projection;

void main()
{
	float tx = in_Tile.x;
	float ty = in_Tile.y;
	float sx = in_Tile.z;
	float sy = in_Tile.w;

	// Warning: matrix is transposed
	mat4 model = mat4(
		vec4(sx,  0,  0,  0),
		vec4( 0, sy,  0,  0),
		vec4( 0,  0,  1,  0),
		vec4(tx, ty,  0,  1)
	);

	in_FragColor = in_Color.bgra;
	in_FragExtra = in_Extra;
	in_FragUV    = in_UV * in_FrameUV.zw + in_FrameUV.xy;
	gl_Position  = projection * model * vec4(in_Vertex, 0.0, 1.0);
}