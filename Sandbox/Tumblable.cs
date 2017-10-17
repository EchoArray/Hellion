using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Echo
{
    public abstract class Tumblable : Impactable
    {
        // Defines the surface response that will be casted upon tumbling.
        [Space(10)]
        [SerializeField] protected SurfaceResponse _tumbleSurfaceResponse;
        // Defines the duration in-which the tumblable will wait to do its next tumble.
        [SerializeField] private float _tumbleResponseCooldown = 1f;
        // Defines the next time that the tumblable is allowed to tumble.
        private float _nextTumbleTime;

        protected override void OnCollisionStay(Collision collision)
        {
            base.OnCollisionStay(collision);
            Tumble(collision.contacts[0], _tumbleSurfaceResponse);
        }
        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            SetNextTumbleTime();
        }
        protected override void OnCollisionExit(Collision collision)
        {
            base.OnCollisionExit(collision);
        }

        private void SetNextTumbleTime()
        {
            _nextTumbleTime = Time.time + _tumbleResponseCooldown;
        }
        protected void Tumble(ContactPoint contactPoint, SurfaceResponse surfaceResponse)
        {
            if (Time.time < _nextTumbleTime)
                return;
            if (surfaceResponse == null)
                return;

            // Raycast to contact point
            RaycastHit raycastHit = RaycastContact(contactPoint);

            // Determine if there is a response effect
            SurfaceResponse.ResponseEffect responseEffect = surfaceResponse.GetResponseEffect(raycastHit, _rigidbody.velocity.magnitude);
            if (responseEffect == null)
                return;
            // Create effect, apply dynamics
            CreateEffect(responseEffect.castable, raycastHit);
            ApplyResponseDynamics(responseEffect, raycastHit);
            // Define the next time that the tumblable is allowed to tumble
            SetNextTumbleTime();
        }
    }
}
