using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;

    public Vector2 Move => controls.Player.Move.ReadValue<Vector2>();
    public bool AttackPressed => controls.Player.Attack.WasPressedThisFrame();
    public bool DashPressed => controls.Player.Dash.WasPressedThisFrame();
    public bool ParryPressed => controls.Player.Parry.WasPressedThisFrame();

    private void Awake() => controls = new PlayerControls();
    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();
    private void OnDestroy() => controls.Dispose();
}