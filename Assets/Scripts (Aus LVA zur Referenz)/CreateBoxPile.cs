using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBoxPile : MonoBehaviour
{
    
    public GameObject blockPrefab;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreatePileCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator CreatePileCoroutine()
    {
        float initHeight = 1.0f;
        float initX = 0.0f;

        int lineElems = 5;
        int runs = 0;
        int lineRuns = 0;

        while (lineElems > 0)
        {
            Vector3 pos = new Vector3(initX, initHeight, 0);
            Instantiate(blockPrefab, pos, transform.rotation);
            yield return new WaitForSeconds(0.3f);

            initX += 1;

            runs++;
            lineRuns++;
            
            if (lineRuns == lineElems)
            {
                Debug.Log("%5 reached");
                initHeight += 1;
                initX = (float)(0 + (0.5 * runs / 5));
                lineElems -= 1;
                lineRuns = 0;
            }
        }
    }
}
