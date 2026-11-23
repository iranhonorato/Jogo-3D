using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera os dois rigs de câmera isométrica do passo 1.2 (GDD §3.1) em Prefabs/Camera/:
    /// perspectiva com FOV baixo e ortográfica, para o usuário escolher. Cada rig tem a Main Camera
    /// com CinemachineBrain e uma CinemachineCamera fixa em X 45° e Y 45°, com Position Composer
    /// (segue suave, com avanço na direção do movimento). Os números se ajustam nos componentes
    /// do Cinemachine do prefab; o Tracking Target (o guaxinim) é ligado na cena.
    /// </summary>
    public static class CameraRigFactory
    {
        private const string PerspectivePath = "Assets/_Project/Prefabs/Camera/CameraRig_Perspective.prefab";
        private const string OrthographicPath = "Assets/_Project/Prefabs/Camera/CameraRig_Orthographic.prefab";
        private const string MainCameraTag = "MainCamera";

        private const float PitchDegrees = 45f;
        private const float YawDegrees = 45f;
        private const float PerspectiveFieldOfView = 25f;
        private const float PerspectiveDistance = 28f;
        private const float OrthographicSize = 6f;
        private const float OrthographicDistance = 20f;
        private const float NearClip = 1f;
        private const float FarClip = 100f;
        private const float FollowDamping = 0.4f;
        private const float LookaheadSeconds = 0.35f;
        private const float LookaheadSmoothing = 0.4f;

        [MenuItem("Tools/Guaxinim/Greybox/Rigs de Câmera Isométrica")]
        private static void CreateCameraRigs()
        {
            SaveRig(BuildRig("CameraRig_Perspective", isOrthographic: false), PerspectivePath);
            SaveRig(BuildRig("CameraRig_Orthographic", isOrthographic: true), OrthographicPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PerspectivePath);
        }

        private static GameObject BuildRig(string name, bool isOrthographic)
        {
            var root = new GameObject(name);
            CreateMainCamera(root.transform, isOrthographic);
            CreateIsometricCamera(root.transform, isOrthographic);
            return root;
        }

        private static void CreateMainCamera(Transform parent, bool isOrthographic)
        {
            var cameraObject = new GameObject("Main Camera") { tag = MainCameraTag };
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.rotation = IsometricRotation;

            var unityCamera = cameraObject.AddComponent<Camera>();
            unityCamera.orthographic = isOrthographic;
            unityCamera.nearClipPlane = NearClip;
            unityCamera.farClipPlane = FarClip;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CinemachineBrain>();
        }

        private static void CreateIsometricCamera(Transform parent, bool isOrthographic)
        {
            var virtualObject = new GameObject("IsometricCamera");
            virtualObject.transform.SetParent(parent, false);
            virtualObject.transform.rotation = IsometricRotation;

            var virtualCamera = virtualObject.AddComponent<CinemachineCamera>();
            LensSettings lens = LensSettings.Default;
            lens.FieldOfView = PerspectiveFieldOfView;
            lens.OrthographicSize = OrthographicSize;
            lens.NearClipPlane = NearClip;
            lens.FarClipPlane = FarClip;
            virtualCamera.Lens = lens; // a projeção vem da Main Camera (ortográfica ou não)

            var composer = virtualObject.AddComponent<CinemachinePositionComposer>();
            composer.CameraDistance = isOrthographic ? OrthographicDistance : PerspectiveDistance;
            composer.Damping = Vector3.one * FollowDamping;
            composer.Lookahead = new LookaheadSettings
            {
                Enabled = true,
                Time = LookaheadSeconds,
                Smoothing = LookaheadSmoothing,
                IgnoreY = true,
            };
        }

        private static Quaternion IsometricRotation => Quaternion.Euler(PitchDegrees, YawDegrees, 0f);

        private static void SaveRig(GameObject rig, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(rig, path);
            Object.DestroyImmediate(rig);
        }
    }
}
