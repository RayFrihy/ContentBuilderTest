using System.Collections.Generic;
using UnityEngine;
using ReactiveFlowEngine.Tests.TestDoubles;

namespace ReactiveFlowEngine.RuntimeTests
{
    public class RuntimeTestHelper
    {
        private readonly List<GameObject> _createdObjects = new List<GameObject>();

        public GameObject CreateGameObject(string name = "TestObject")
        {
            var go = new GameObject(name);
            _createdObjects.Add(go);
            return go;
        }

        public GameObject CreateGameObjectAt(string name, Vector3 position)
        {
            var go = CreateGameObject(name);
            go.transform.position = position;
            return go;
        }

        public GameObject CreateGameObjectWithRotation(string name, Vector3 position, Quaternion rotation)
        {
            var go = CreateGameObject(name);
            go.transform.position = position;
            go.transform.rotation = rotation;
            return go;
        }

        public GameObject CreateGameObjectWithRenderer(string name = "RenderObject")
        {
            var go = CreateGameObject(name);
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateSimpleMesh();
            go.AddComponent<MeshRenderer>();
            return go;
        }

        public GameObject CreateGameObjectWithAudioSource(string name = "AudioObject")
        {
            var go = CreateGameObject(name);
            go.AddComponent<AudioSource>();
            return go;
        }

        public GameObject CreateGameObjectWithAnimator(string name = "AnimatorObject")
        {
            var go = CreateGameObject(name);
            go.AddComponent<Animator>();
            return go;
        }

        public GameObject CreateGameObjectWithCollider(string name = "ColliderObject")
        {
            var go = CreateGameObject(name);
            go.AddComponent<BoxCollider>();
            return go;
        }

        public GameObject CreateGameObjectWithParticleSystem(string name = "ParticleObject")
        {
            var go = CreateGameObject(name);
            go.AddComponent<ParticleSystem>();
            return go;
        }

        public void RegisterWithResolver(MockSceneObjectResolver resolver, string guid, Transform transform)
        {
            resolver.Register(guid, transform);
        }

        public void TearDown()
        {
            foreach (var go in _createdObjects)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            _createdObjects.Clear();
        }

        private Mesh CreateSimpleMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            return mesh;
        }
    }
}
