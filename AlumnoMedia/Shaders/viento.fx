/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

float time = 0;


/**************************************************************************************/
/* RenderScene */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT 
{
   float4 Position : POSITION0;
   float4 Color : COLOR0;
   float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float4 Color :			COLOR0;
};



//Vertex Shader
VS_OUTPUT vs_main( VS_INPUT Input )
{
   VS_OUTPUT Output;
   //Proyectar posicion
   Output.Position = mul( Input.Position, matWorldViewProj);
   
   //Propago las coordenadas de textura
   Output.Texcoord = Input.Texcoord;

   //Propago el color x vertice
   Output.Color = Input.Color;

   return( Output );
   
}


// Ejemplo de un vertex shader que anima la posicion de los vertices 
// ------------------------------------------------------------------
VS_OUTPUT vs_main2( VS_INPUT Input )
{
   VS_OUTPUT Output;

   // Animar posicion
   /*Input.Position.x += sin(time)*30*sign(Input.Position.x);
   Input.Position.y += cos(time)*30*sign(Input.Position.y-20);
   Input.Position.z += sin(time)*30*sign(Input.Position.z);
   */

   // Animar posicion
   float4 displ;
   float dx = 0.0;
   float dy = 0.0;
   float PI = 3.1415;
   float t = time * 0.08;

   if(Input.Position.y >= 115){
      dx += 25*(cos(t*PI)*cos(t*PI)*cos(t*1*PI)*cos(t*3*PI)*cos(t*5*PI) + sin(t*2*PI) * 0.1);
   }

   if (Input.Position.y >= 175){
      dy += 25 *((cos(t*PI)*cos(t*PI)) *cos(3*t*PI)*cos(5*t*PI)*0.5 + sin(t*25*PI)*0.02);
   }

   displ.x = Input.Position.x + dx;
   displ.y = Input.Position.y + dy;
   displ.z = Input.Position.z;
   displ.w = 1.0;
   //Input.Position.y = sin(Y+(time*10)) * (Z-Y) * 0.1;

   //Proyectar posicion
   Output.Position = mul( displ, matWorldViewProj);
   
   //Propago las coordenadas de textura
   Output.Texcoord = Input.Texcoord;

	/*// Animar color
   Input.Color.r = abs(sin(time));
   Input.Color.g = abs(cos(time));*/
   
   //Propago el color x vertice
   Output.Color = Input.Color;

   return( Output );
   
}

//Pixel Shader
float4 ps_main( float2 Texcoord: TEXCOORD0, float4 Color:COLOR0) : COLOR0
{      
	// Obtener el texel de textura
	// diffuseMap es el sampler, Texcoord son las coordenadas interpoladas
	float4 fvBaseColor = tex2D( diffuseMap, Texcoord );
	// combino color y textura
	// en este ejemplo combino un 80% el color de la textura y un 20%el del vertice
	return fvBaseColor;
}


// ------------------------------------------------------------------
technique RenderScene
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_main2();
	  PixelShader = compile ps_2_0 ps_main();
   }

}
