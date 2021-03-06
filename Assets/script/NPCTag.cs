﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCTag : MonoBehaviour {

	NPCTag _target;
	Vector3 _wallTarget;
	public bool _flee;
	public Vector3 _velocity;
	public int _id;
	public bool _frozen = false;
	public bool _hunter = false;
	Vector3 _rotvelocity;
	bool kinematic;
	[SerializeField]
	float _t2t = 0.5f;
	[SerializeField]
	float _maxSpeed = 2f;
	[SerializeField]
	float _maxAcc = 0.1f;
	[SerializeField]
	float _maxRotSpeed = 40f;
	[SerializeField]
	float _fastLimit = 0.5f;
	[SerializeField]
	float _nearRadius = 0.5f;
	[SerializeField]
	float _angleView = 10f;

	private void Start () {
		_frozen = false;
		_velocity = new Vector3(0, 0, 0);
		_rotvelocity = new Vector3 (0, 0, 0);
		kinematic = true;
	}

	private void Update() { // Swapping between kinematic and steering behaviors
		if (Input.GetKeyDown ("k"))
			kinematic = true;
		else if (Input.GetKeyDown ("s"))
			kinematic = false;
	}

	private void FixedUpdate () {
		if (!_frozen) {
			// Recalculate the target position through wall, with inclusion of the velocity of the target for the steering behavior
			_wallTarget = _target.transform.position;
			if (!kinematic)
				_wallTarget += _target._velocity;
			if (_wallTarget.x - transform.position.x > 4.5f) // left
				_wallTarget.x -= 10f;
			else if (_wallTarget.x - transform.position.x < -4.5f) // Right
				_wallTarget.x += 10f;
			if (_wallTarget.z - transform.position.z > 4.5f) // Down
				_wallTarget.z -= 10f;
			else if (_wallTarget.z - transform.position.z < -4.5f) // Up
				_wallTarget.z += 10f;

			// Activate the good behavior
			if (kinematic)
				KinematicBehaviorHandler ();
			else
				SteeringBehaviorHandler ();

			// Rotate and Move as required
			if (!float.IsNaN(_rotvelocity.y) && !float.IsInfinity(_rotvelocity.y))
				transform.eulerAngles = transform.eulerAngles + _rotvelocity * Time.deltaTime * (_hunter ? 1.4f : 1f);
			transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
			transform.position += _velocity * Time.deltaTime * (_hunter ? 1.4f : 1f);
			transform.position = new Vector3 (transform.position.x, 0, transform.position.z);
		}
	}

	private void KinematicBehaviorHandler() {
		Vector3 vecdist = transform.position - _wallTarget;
		float dist = Mathf.Sqrt (vecdist.x * vecdist.x + vecdist.z * vecdist.z);
		float spd = Mathf.Sqrt (_velocity.x * _velocity.x + _velocity.z * _velocity.z);

		// A: If we are slow
		if (_flee || spd < _fastLimit) {
			// A.i: If we are near the target
			if (dist < _nearRadius)
				kMoveToward (); // We move toward it
			else { // A.ii: If we are far from the target
				float angle = kRotateToward ();
				// If it's in a close angle
				if ((!_flee && Mathf.Abs(angle) < _angleView) || (_flee && 180 - Mathf.Abs(angle) < _angleView))
					kMoveForward (); // We move forward
				else // if it's not
					Stop();
				if (_flee)
					_velocity = -_velocity;
			}
		} else { // B: If we are fast
			float angle = kRotateToward ();
			// B.i: If we are in a close angle
			if (Mathf.Abs(angle) < _angleView)
				kMoveForward ();
			else // B.ii: If we are not in a close angle
				Stop();
		}

		if (_flee) {// C: invert the rotation and the direction
			_velocity = -_velocity;
			_rotvelocity = -_rotvelocity;
		}
	}

	private void SteeringBehaviorHandler() {
		Vector3 vecdist = transform.position - _wallTarget;
		float dist = Mathf.Sqrt (vecdist.x * vecdist.x + vecdist.z * vecdist.z);
		float spd = Mathf.Sqrt (_velocity.x * _velocity.x + _velocity.z * _velocity.z);

		// A: If we are slow
		if (_flee || spd < _fastLimit) {
			// A.i: If we are near the target
			if (dist < _nearRadius)
				sMoveToward (); // We move toward it
			else { // A.ii: If we are far from the target
				float angle = sRotateToward ();
				// If it's in a close angle
				if ((!_flee && Mathf.Abs(angle) < _angleView) || (_flee && 180 - Mathf.Abs(angle) < _angleView))
					sMoveForward (); // We move forward
				else // if it's not
					Stop();
			}
		} else { // B: If we are fast
			float angle = sRotateToward ();
			// B.i: If we are in a close angle
			if (Mathf.Abs(angle) < _angleView)
				sMoveForward ();
			else // B.ii: If we are not in a close angle
				Stop();
		}

		if (_flee) {// C: invert the rotation
			_rotvelocity = -_rotvelocity;
		}
	}

	public void setTarget(NPCTag target, bool flee) {
		_target = target;
		_flee = flee;
	}

	//Required Basic Functions On Vector
	private Vector3 Normalize(Vector3 v) {
		float a = Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);
		return ((a == 0) ? (v) : (v / a));
	}

	private Vector3 Opposite(Vector3 target) {
		return (2 * transform.position - target);
	}

	//Required Basic Functions on Movement
	private void Stop()	{
		_velocity /= _maxSpeed;
	}

	//Required Basic Functions On Kinematic Movement
	private float kRotateToward() {
		Vector3 veldir = _wallTarget - transform.position;
		Vector3 forward = transform.forward;
		float angle = Mathf.Atan2(forward.z * veldir.x - veldir.z * forward.x, forward.x * veldir.x + forward.z * veldir.z) * Mathf.Rad2Deg;
		if (float.IsNaN (angle))
			angle = 0f;
		_rotvelocity = new Vector3 (0, 5 * Mathf.Min(Mathf.Abs(angle), _maxRotSpeed) * angle / Mathf.Abs(angle), 0);
		return (angle);
	}

	private void kMoveForward() {
		_velocity = transform.forward * _maxSpeed;
	}

	private void kMoveToward() {
		Vector3 veldir = _wallTarget - transform.position;
		veldir = Normalize (veldir);
		_velocity = veldir * _maxSpeed;
	}

	//Required Basic Functions On Steering Movement
	private float sRotateToward() {
		Vector3 veldir = _wallTarget - transform.position;
		Vector3 forward = transform.forward;
		float angle = Mathf.Atan2(forward.z * veldir.x - veldir.z * forward.x, forward.x * veldir.x + forward.z * veldir.z) * Mathf.Rad2Deg;
		if (float.IsNaN (angle))
			angle = 0f;
		Vector3 desired = new Vector3 (0, 5 * Mathf.Min(Mathf.Abs(angle), _maxRotSpeed) * angle / Mathf.Abs(angle), 0);
		Vector3 steering = desired - _rotvelocity;
		_rotvelocity += steering;
		return (angle);
	}

	private void sMoveForward() {
		Vector3 veldir = _wallTarget - transform.position;
		float spd = Mathf.Sqrt (veldir.x * veldir.x + veldir.z * veldir.z) / _t2t;
		Vector3 desired = transform.forward * Mathf.Min(_maxSpeed, spd);
		Vector3 steering = desired - _velocity;
		float acc = Mathf.Sqrt (steering.x * steering.x + steering.z * steering.z) / _t2t;
		steering = Normalize(steering) * Mathf.Min(_maxAcc, acc);
		_velocity += steering;
		_velocity = Normalize (_velocity) * _maxSpeed;
			print (_velocity);
	}

	private void sMoveToward() {
		Vector3 veldir = _wallTarget - transform.position;
		float spd = Mathf.Sqrt (veldir.x * veldir.x + veldir.z * veldir.z) / _t2t;
		veldir = Normalize (veldir);
		Vector3 desired = veldir * Mathf.Min(_maxSpeed, spd);
		if (_flee)
			desired = desired * -1;
		Vector3 steering = desired - _velocity;
		float acc = Mathf.Sqrt (steering.x * steering.x + steering.z * steering.z) / _t2t;
		steering = Normalize(desired - _velocity) * Mathf.Min(_maxAcc, acc);
		_velocity += steering;
		_velocity = Normalize (_velocity) * _maxSpeed;
	}

	private void OnCollisionEnter(Collision collision) { // Freeze and Unfreeze on collision
		if (_hunter && collision.collider.tag == "NPC" && !collision.gameObject.GetComponent<NPCTag>()._frozen)
			transform.parent.gameObject.GetComponent<tagHandler>().reduceNbPlayer(collision.gameObject.GetComponent<NPCTag>()._id);
		if (!_hunter && collision.collider.tag == "NPC" && collision.gameObject.GetComponent<NPCTag>()._frozen)
			transform.parent.gameObject.GetComponent<tagHandler>().incrementNbPlayer(collision.gameObject.GetComponent<NPCTag>()._id);
	}

}
