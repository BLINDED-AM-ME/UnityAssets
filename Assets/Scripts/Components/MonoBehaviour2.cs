using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using BLINDED_AM_ME.Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BLINDED_AM_ME.Components
{
	/// <summary> I want to use Overrides and Properties </summary>
	public class MonoBehaviour2 : MonoBehaviour, INotifyPropertyChanged
	{
		[HideInInspector]
		[SerializeField]
		[SerializeProperty(nameof(Id))]
		private string _id;
		public string Id
		{
			get => _id;
			set => SetProperty(ref _id, value);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <returns> true if property was changed </returns>
		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (Equals(storage, value))
				return false;

			storage = value;
			OnPropertyChanged(propertyName);
			return true;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary> Awake is called when the script instance is being loaded</summary>
		protected virtual void Awake() { }
		/// <summary> This function is called when the object becomes enabled and active.</summary>
		protected virtual void OnEnable() { }
		/// <summary> Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.</summary>
		protected virtual void Start() { }

		/// <summary> Update is called every frame, if the MonoBehaviour is enabled.</summary>
		protected virtual void Update() { }
		/// <summary> Frame-rate independent MonoBehaviour.FixedUpdate message for physics calculations.</summary>
		protected virtual void FixedUpdate() { }
		/// <summary> LateUpdate is called every frame, if the Behaviour is enabled. </summary>
		protected virtual void LateUpdate() { }
		
		/// <summary> This function is called when the behaviour becomes disabled.</summary>
		protected virtual void OnDisable() { }
		/// <summary> Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy.</summary>
		protected virtual void OnDestroy() { }


		/// <summary> Reset to default values. </summary>
		protected virtual void Reset() { }

		/// <summary>
		/// Callback for setting up animation IK (inverse kinematics).
		/// </summary>
		protected virtual void OnAnimatorIK() { }
		/// <summary>
		/// Callback for processing animation movements for modifying root motion.
		/// </summary>
		protected virtual void OnAnimatorMove() { }
		/// <summary>
		/// Sent to all GameObjects when the player gets or loses focus.
		/// </summary>
		protected virtual void OnApplicationFocus() { }
		/// <summary>
		/// Sent to all GameObjects when the application pauses.
		/// </summary>
		protected virtual void OnApplicationPause() { }
		/// <summary>
		/// Sent to all GameObjects before the application quits.
		/// </summary>
		protected virtual void OnApplicationQuit() { }
		/// <summary>
		/// If OnAudioFilterRead is implemented, Unity will insert a custom filter into the audio DSP chain.
		/// </summary>
		protected virtual void OnAudioFilterRead(float[] data, int channels) { }
		/// <summary>
		/// OnBecameInvisible is called when the renderer is no longer visible by any camera.
		/// </summary>
		protected virtual void OnBecameInvisible() { }
		/// <summary>
		/// OnBecameVisible is called when the renderer became visible by any camera.
		/// </summary>
		protected virtual void OnBecameVisible() { }
		/// <summary>
		/// OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
		/// </summary>
		protected virtual void OnCollisionEnter() { }
		/// <summary>
		/// Sent when an incoming collider makes contact with this object's collider (2D physics only).
		/// </summary>
		protected virtual void OnCollisionEnter2D() { }
		/// <summary>
		/// OnCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider.
		/// </summary>
		protected virtual void OnCollisionExit() { }
		/// <summary>
		/// Sent when a collider on another object stops touching this object's collider (2D physics only).
		/// </summary>
		protected virtual void OnCollisionExit2D() { }
		/// <summary>
		/// OnCollisionStay is called once per frame for every Collider or Rigidbody that touches another Collider or Rigidbody.
		/// </summary>
		protected virtual void OnCollisionStay() { }
		/// <summary>
		/// Sent each frame where a collider on another object is touching this object's collider (2D physics only).
		/// </summary>
		protected virtual void OnCollisionStay2D() { }
		/// <summary>
		/// Called on the client when you have successfully connected to a server.
		/// </summary>
		protected virtual void OnConnectedToServer() { }
		/// <summary>
		/// OnControllerColliderHit is called when the controller hits a collider while performing a Move.
		/// </summary>
		protected virtual void OnControllerColliderHit() { }
		/// <summary>
		/// Called on the client when the connection was lost or you disconnected from the server.
		/// </summary>
		protected virtual void OnDisconnectedFromServer() { }
		/// <summary>
		/// Implement OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn.
		/// </summary>
		protected virtual void OnDrawGizmos() { }
		/// <summary>
		/// Implement OnDrawGizmosSelected to draw a gizmo if the object is selected.
		/// </summary>
		protected virtual void OnDrawGizmosSelected() { }
		/// <summary>
		/// Called on the client when a connection attempt fails for some reason.
		/// </summary>
		protected virtual void OnFailedToConnect() { }
		/// <summary>
		/// Called on clients or servers when there is a problem connecting to the MasterServer.
		/// </summary>
		protected virtual void OnFailedToConnectToMasterServer() { }
		/// <summary>
		/// OnGUI is called for rendering and handling GUI events.
		/// </summary>
		protected virtual void OnGUI() { }
		/// <summary>
		/// Called when a joint attached to the same game object broke.
		/// </summary>
		protected virtual void OnJointBreak() { }
		/// <summary>
		/// Called when a Joint2D attached to the same game object breaks.
		/// </summary>
		protected virtual void OnJointBreak2D() { }
		/// <summary>
		/// Called on clients or servers when reporting events from the MasterServer.
		/// </summary>
		protected virtual void OnMasterServerEvent() { }
		/// <summary>
		/// OnMouseDown is called when the user has pressed the mouse button while over the Collider.
		/// </summary>
		protected virtual void OnMouseDown() { }
		/// <summary>
		/// OnMouseDrag is called when the user has clicked on a Collider and is still holding down the mouse.
		/// </summary>
		protected virtual void OnMouseDrag() { }
		/// <summary>
		/// Called when the mouse enters the Collider.
		/// </summary>
		protected virtual void OnMouseEnter() { }
		/// <summary>
		/// Called when the mouse is not any longer over the Collider.
		/// </summary>
		protected virtual void OnMouseExit() { }
		/// <summary>
		/// Called every frame while the mouse is over the Collider.
		/// </summary>
		protected virtual void OnMouseOver() { }
		/// <summary>
		/// OnMouseUp is called when the user has released the mouse button.
		/// </summary>
		protected virtual void OnMouseUp() { }
		/// <summary>
		/// OnMouseUpAsButton is only called when the mouse is released over the same Collider as it was pressed.
		/// </summary>
		protected virtual void OnMouseUpAsButton() { }
		/// <summary>
		/// Called on objects which have been network instantiated with Network.Instantiate.
		/// </summary>
		protected virtual void OnNetworkInstantiate() { }
		/// <summary>
		/// OnParticleCollision is called when a particle hits a Collider.
		/// </summary>
		protected virtual void OnParticleCollision() { }
		/// <summary>
		/// OnParticleSystemStopped is called when all particles in the system have died, and no new particles will be born.New particles cease to be created either after Stop is called, or when the duration property of a non-looping system has been exceeded.
		/// </summary>
		protected virtual void OnParticleSystemStopped() { }
		/// <summary>
		/// OnParticleTrigger is called when any particles in a Particle System meet the conditions in the trigger module.
		/// </summary>
		protected virtual void OnParticleTrigger() { }
		/// <summary>
		/// OnParticleUpdateJobScheduled is called when a Particle System's built-in update job has been scheduled.
		/// </summary>
		protected virtual void OnParticleUpdateJobScheduled() { }
		/// <summary>
		/// Called on the server whenever a new player has successfully connected.
		/// </summary>
		protected virtual void OnPlayerConnected() { }
		/// <summary>
		/// Called on the server whenever a player disconnected from the server.
		/// </summary>
		protected virtual void OnPlayerDisconnected() { }
		/// <summary>
		/// Event function that Unity calls after a Camera renders the scene.
		/// </summary>
		protected virtual void OnPostRender() { }
		/// <summary>
		/// Event function that Unity calls before a Camera culls the scene.
		/// </summary>
		protected virtual void OnPreCull() { }
		/// <summary>
		/// Event function that Unity calls before a Camera renders the scene.
		/// </summary>
		protected virtual void OnPreRender() { }
		/// <summary>
		/// Event function that Unity calls after a Camera has finished rendering, that allows you to modify the Camera's final image.
		/// </summary>
		protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination) { }
		/// <summary>
		/// OnRenderObject is called after camera has rendered the Scene.
		/// </summary>
		protected virtual void OnRenderObject() { }
		/// <summary>
		/// Used to customize synchronization of variables in a script watched by a network view.
		/// </summary>
		protected virtual void OnSerializeNetworkView() { }
		/// <summary>
		/// Called on the server whenever a Network.InitializeServer was invoked and has completed.
		/// </summary>
		protected virtual void OnServerInitialized() { }
		/// <summary>
		/// This function is called when the list of children of the transform of the GameObject has changed.
		/// </summary>
		protected virtual void OnTransformChildrenChanged() { }
		/// <summary>
		/// This function is called when a direct or indirect parent of the transform of the GameObject has changed.
		/// </summary>
		protected virtual void OnTransformParentChanged() { }
		/// <summary>
		/// When a GameObject collides with another GameObject, Unity calls OnTriggerEnter.
		/// </summary>
		protected virtual void OnTriggerEnter() { }
		/// <summary>
		/// Sent when another object enters a trigger collider attached to this object (2D physics only).
		/// </summary>
		protected virtual void OnTriggerEnter2D() { }
		/// <summary>
		/// OnTriggerExit is called when the Collider other has stopped touching the trigger.
		/// </summary>
		protected virtual void OnTriggerExit() { }
		/// <summary>
		/// Sent when another object leaves a trigger collider attached to this object (2D physics only).
		/// </summary>
		protected virtual void OnTriggerExit2D() { }
		/// <summary>
		/// OnTriggerStay is called once per physics update for every Collider other that is touching the trigger.
		/// </summary>
		protected virtual void OnTriggerStay() { }
		/// <summary>
		/// Sent each frame where another object is within a trigger collider attached to this object (2D physics only).
		/// </summary>
		protected virtual void OnTriggerStay2D() { }
		/// <summary>
		/// Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
		/// </summary>
		protected virtual void OnValidate() { }
		/// <summary>
		/// OnWillRenderObject is called for each camera if the object is visible and not a UI element.
		/// </summary>
		protected virtual void OnWillRenderObject() { }
		
	}
}