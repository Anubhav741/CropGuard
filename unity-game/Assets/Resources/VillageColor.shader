Shader "Jalrakshak/VillageColor" {
 Properties { _Color ("Color", Color) = (1,1,1,1) }
 SubShader {
  Tags { "RenderType"="Opaque" }
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
   struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 world:TEXCOORD1; };
   fixed4 _Color; float _DayLight;
   v2f vert(appdata v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.normal=UnityObjectToWorldNormal(v.normal); o.world=mul(unity_ObjectToWorld,v.vertex).xyz; return o; }
   fixed4 frag(v2f i):SV_Target { float shade=.67+.33*saturate(dot(normalize(i.normal),normalize(float3(-.4,.85,-.3)))); return fixed4(_Color.rgb*shade*max(.55,_DayLight),1); }
   ENDCG
  }
 }
}
