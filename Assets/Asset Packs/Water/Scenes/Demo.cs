using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour {
    public Material mat;
    public Material CustomMat;
    public Renderer Plane;
    public void Customize()
    {
        Plane.material = CustomMat;
    }

    public void Back()
    {
        Plane.material = mat;
    }

    public void NewColor(string c)
    {
        if(c == "red")
        {
            Color32 r = Color.red;
            r.r = 164;
            r.g = 19;
            r.b = 19;
            r.a = 255;
            CustomMat.SetColor("_Polygonal_Color", r);
        }
        if (c == "blue")
        {
            Color32 b = Color.blue;
            b.r = 12;
            b.g = 104;
            b.b = 191;
            b.a = 255;

            CustomMat.SetColor("_Polygonal_Color", b);
        }
        if (c == "green")
        {
            Color32 g = Color.green;
            g.r = 49;
            g.g = 202;
            g.b = 49;
            g.a = 255;

            CustomMat.SetColor("_Polygonal_Color", g);
        }
        if (c == "yellow")
        {
            CustomMat.SetColor("_Polygonal_Color", Color.yellow);
        }
        if (c == "white")
        {
            Color32 w = Color.white;
            w.r = 190;
            w.g = 190;
            w.b = 190;
            w.a = 255;
            CustomMat.SetColor("_Polygonal_Color", w);
        }
    }

    public void NewSize (InputField s)
    {
        if(s.text != null)
        {
            CustomMat.SetFloat("_Polygonal_Size", float.Parse(s.text));
        }
        
    }
    public void NewSpeed(InputField s)
    {
        if (s.text != null)
        {
            CustomMat.SetVector("_Polygonal_Speed", new Vector4(0, float.Parse(s.text), 0,0));
        }

    }
    public void NewGlowSize(Slider s)
    {
        CustomMat.SetFloat("_Glow_Size", s.value);
    }

    public void NewDetail(InputField d)
    {
        if (d.text != null)
        {
            CustomMat.SetFloat("_Detail_Speed", float.Parse(d.text));
        }
    }

    public void NewWaves(Slider w)
    {
        CustomMat.SetFloat("_Waves", w.value);
    }
    public void NewAlpha(Slider a)
    {
        CustomMat.SetFloat("_Alpha", a.value);
    }

}
