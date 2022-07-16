extends RigidBody

var turn_speed = Vector3(1,0,0)

var movement_speed1 = Vector3(1,0,0)
var movement_speed = Vector3(0,0,1)

# The direction in which to conduct a raycast to check for jumping
var floor_jump_dir = Vector3(0,1,0)

var jump_enabled = true
var jump_speed = Vector3(0,3,0)

const JUMP_RAYCAST_CHECK_RANGE = 1

# Add input action dependencies if not present
func _ready():
	if not InputMap.has_action("turn_left"):
		InputMap.add_action("turn_left")
		var left_key = InputEventKey.new()
		left_key.set_scancode(KEY_LEFT)
		InputMap.action_add_event("turn_left",left_key)
		
	if not InputMap.has_action("turn_right"):
		InputMap.add_action("turn_right")
		var right_key = InputEventKey.new()
		right_key.set_scancode(KEY_RIGHT)
		InputMap.action_add_event("turn_right", right_key)
		
	if not InputMap.has_action("move_forward"):
		InputMap.add_action("move_forward")
		var up_key = InputEventKey.new()
		up_key.set_scancode(KEY_UP)
		InputMap.action_add_event("move_forward", up_key)	
		
	if not InputMap.has_action("move_backward"):
		InputMap.add_action("move_backward")
		var down_key = InputEventKey.new()
		down_key.set_scancode(KEY_DOWN)
		InputMap.action_add_event("move_backward", down_key)
		
	if not InputMap.has_action("jump"):
		InputMap.add_action("jump")
		var space_key = InputEventKey.new()
		space_key.set_scancode(KEY_SPACE)
		InputMap.action_add_event("jump", space_key)

# The documentation of Rigidbody advises against adding
# physics forces during _input, suggesting to use
# _integrate_forces instead.
# http://docs.godotengine.org/en/latest/classes/class_rigidbody.html#class-rigidbody-set-angular-velocity
func _integrate_forces(state):
	#state.set_angular_velocity(-turn_speed)
	
	# Detect input and alter the state accordingly
	if Input.is_action_pressed("turn_left"):
		state.set_linear_velocity(-movement_speed1)
	if Input.is_action_pressed("turn_right"):
		state.set_linear_velocity(movement_speed1)
	if Input.is_action_pressed("move_forward"):
		state.set_linear_velocity(-movement_speed)
	if Input.is_action_pressed("move_backward"):
		state.set_linear_velocity(movement_speed)
	if Input.is_action_pressed("jump") and can_jump(state):
		state.set_linear_velocity(jump_speed)

func can_jump(state):
	# Use raycasting start at points surrounding the model,
	# pointed downwards to determine if ground is below.
	# Measure how far the rays go before they collide with anything.
	# Should be pretty short.
	# [self] is to preclude the raycast from hitting yourself
	var hit = state.get_space_state().intersect_ray(global_transform.origin, global_transform.origin + JUMP_RAYCAST_CHECK_RANGE * -floor_jump_dir, [self])
	if hit.size() == 0:
		print("miss")
		return false
	if hit.collider.is_in_group("floor"):
		print("JUMP")
		return true
	
	print("CANT JUMP")
	return false
