// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "hex_border_shader"
{
	Properties
	{
		_texture("texture", 2D) = "white" {}
		_emission_power("emission_power", Range( 0 , 5)) = 1
		_color("color", Color) = (1,1,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		AlphaToMask On
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow nolightmap  nodynlightmap nodirlightmap nofog 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _emission_power;
		uniform sampler2D _texture;
		uniform float4 _texture_ST;
		uniform float4 _color;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_texture = i.uv_texcoord * _texture_ST.xy + _texture_ST.zw;
			float4 tex2DNode1 = tex2D( _texture, uv_texture );
			o.Emission = ( _emission_power * ( tex2DNode1 * _color ) ).rgb;
			o.Alpha = tex2DNode1.a;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
1182;73;843;812;1038.683;424.8131;1.3;True;False
Node;AmplifyShaderEditor.ColorNode;6;-569.0992,26.50001;Inherit;False;Property;_color;color;2;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-663.2997,-214.3001;Inherit;True;Property;_texture;texture;0;0;Create;True;0;0;0;False;0;False;-1;d65fd9f57763812448b39cb161771577;d65fd9f57763812448b39cb161771577;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-206.3003,-184.5002;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-376.2992,273.9999;Inherit;False;Property;_emission_power;emission_power;1;0;Create;True;0;0;0;False;0;False;1;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-3.9,-186.0002;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;241,-189.8;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;hex_border_shader;False;False;False;False;False;False;True;True;True;True;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;True;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;1;0
WireConnection;3;1;6;0
WireConnection;7;0;2;0
WireConnection;7;1;3;0
WireConnection;0;2;7;0
WireConnection;0;9;1;4
ASEEND*/
//CHKSM=63FCAA0821E2747E8048E636243C76BAD88AEFC6