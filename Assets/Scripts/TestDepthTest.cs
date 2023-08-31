using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDepthTest : MonoBehaviour
{
    private MeshRenderer myRenderer;
    private void Awake(){
        myRenderer = GetComponent<MeshRenderer>();
    }
    public void TestMehthod(bool func){
        int id = Shader.PropertyToID(name:"_Health");
        foreach(Material material in myRenderer.materials){
            if(func)
                material.SetFloat(id,material.GetFloat(id) + 0.125f);
            else
                material.SetFloat(id,material.GetFloat(id) - 0.125f);
        }
    }
}
