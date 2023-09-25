using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Mirror.Examples.Pong
{
    public class DestructableObject : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _destructionParticle;
        [SerializeField] private float _objLife;

        //[Command]
        public void Damage()
        {
            /*if(!isClient)
            {
                RpcDamage();
            }*/
            _objLife--;
            _destructionParticle.Play();

            if(_objLife == 0)
            {
                StartCoroutine(DestroyObject());
            }
        }

        //[ClientRpc]
        /*private void RpcDamage()
        {
            _objLife--;
            _destructionParticle.Play();

            if(_objLife == 0)
            {
                StartCoroutine(DestroyObject());
            }
        }
*/
        private IEnumerator DestroyObject()
        {
            yield return new WaitForSeconds(0.8f);
            Destroy(gameObject);
        }
    }
}