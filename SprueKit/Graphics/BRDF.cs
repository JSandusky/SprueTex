using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace SprueKit.Graphics
{
    public class BRDF
    {

        static Vector3 SchlickFresnel(Vector3 specular, float VdotH)
        {
            return specular + (new Vector3(1.0f, 1.0f, 1.0f) - specular) * Mathf.Pow(1.0f - VdotH, 5.0f);
        }

        public static Vector3 Fresnel(Vector3 specular, float VdotH, float LdotH)
        {
            //return SchlickFresnelCustom(specular, LdotH);
            return SchlickFresnel(specular, VdotH);
        }

        static float NeumannVisibility(float NdotV, float NdotL)
        {
            return (float)(NdotL * NdotV / Math.Max(1e-7, Math.Max(NdotL, NdotV)));
        }

        public static float Visibility(float NdotL, float NdotV, float roughness)
        {
            return NeumannVisibility(NdotV, NdotL);
            //return SmithGGXSchlickVisibility(NdotL, NdotV, roughness);
        }

        static float GGXDistribution(float NdotH, float roughness)
        {
            float rough2 = roughness * roughness;
            float tmp = (NdotH * rough2 - NdotH) * NdotH + 1;
            return rough2 / (tmp * tmp);
        }

        public static float Distribution(float NdotH, float roughness)
        {
            return GGXDistribution(NdotH, roughness);
        }

        static Vector3 BurleyDiffuse(Vector3 diffuseColor, float roughness, float NdotV, float NdotL, float VdotH)
        {
            float energyBias = Mathf.Lerp(0, 0.5f, roughness);
            float energyFactor = Mathf.Lerp(1.0f, 1.0f / 1.51f, roughness);
            float fd90 = energyBias + 2.0f * VdotH * VdotH * roughness;
            float f0 = 1.0f;
            float lightScatter = f0 + (fd90 - f0) * Mathf.Pow(1.0f - NdotL, 5.0f);
            float viewScatter = f0 + (fd90 - f0) * Mathf.Pow(1.0f - NdotV, 5.0f);

            return diffuseColor * lightScatter * viewScatter * energyFactor;
        }

        public static Vector3 Diffuse(Vector3 diffuseColor, float roughness, float NdotV, float NdotL, float VdotH)
        {
            //return LambertianDiffuse(diffuseColor);
            //return CustomLambertianDiffuse(diffuseColor, NdotV, roughness);
            return BurleyDiffuse(diffuseColor, roughness, NdotV, NdotL, VdotH);
        }

        public static Vector3 GetBRDF(Vector3 worldPos, Vector3 lightDir, Vector3 lightVec, Vector3 toCamera, Vector3 normal, float roughness, Vector3 diffColor, Vector3 specColor)
        {
            Vector3 Hn = Vector3.Normalize(toCamera + lightDir);
            float vdh = Mathf.Clamp((Vector3.Dot(toCamera, Hn)), Mathf.EPSILON, 1.0f);
            float ndh = Mathf.Clamp((Vector3.Dot(normal, Hn)), Mathf.EPSILON, 1.0f);
            float ndl = Mathf.Clamp((Vector3.Dot(normal, lightVec)), Mathf.EPSILON, 1.0f);
            float ndv = Mathf.Clamp((Vector3.Dot(normal, toCamera)), Mathf.EPSILON, 1.0f);
            float ldh = Mathf.Clamp((Vector3.Dot(lightVec, Hn)), Mathf.EPSILON, 1.0f);

            Vector3 diffuseFactor = Diffuse(diffColor, roughness, ndv, ndl, vdh) * ndl;
            Vector3 specularFactor = Vector3.Zero;

            Vector3 fresnelTerm = Fresnel(specColor, vdh, ldh);
            float distTerm = Distribution(ndh, roughness);
            float visTerm = Visibility(ndl, ndv, roughness);
            specularFactor = distTerm * visTerm * fresnelTerm * ndl / Mathf.PI;
            return diffuseFactor + specularFactor;// * specColor;
        }
    }
}
