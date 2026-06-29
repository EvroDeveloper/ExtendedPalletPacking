using System.Collections.Generic;
using EvroDev.FileModLib;
using SLZ.Marrow.Warehouse;

namespace LabWorks.ExtendedPalletPacking
{
    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/CrateEditor.cs")]
    public class CrateEditorBuildFlagsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(33, "SerializedProperty excludeAssetsProperty;"));
            modRequests.Add(new InsertLineRequest(57, "excludeAssetsProperty = serializedObject.FindProperty(\"_excludeAssets\");"));
            modRequests.Add(new InsertLineRequest(130, "EditorGUILayout.PropertyField(excludeAssetsProperty);"));
            return modRequests.ToArray();
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow/SLZ.Marrow.Warehouse/Crate.cs")]
    public class CrateBuildFlagsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(21, @"
        public static bool UseFallbackGuid { get; set; }
        public virtual bool EnableFallback { get; set; }
        public virtual SpawnableCrateReference FallbackCrate { get; set; }
        [SerializeField]
        private bool _excludeAssets;
        public virtual bool ExcludeMainAsset { get => _excludeAssets; set => _excludeAssets = value; }"));
            modRequests.Add(new DeleteLineRequest(103));
            modRequests.Add(new InsertLineRequest(103, @"
            if (UseFallbackGuid && EnableFallback)
            {
                json.Add(""mainAsset"", FallbackCrate.Crate.MainAsset.AssetGUID);
            }
            else
            {
                json.Add(""mainAsset"", MainAsset.AssetGUID);
            }
            "));
            return modRequests.ToArray();
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow/SLZ.Marrow.Warehouse/GameObjectCrate.cs")]
    public class GameObjectCrateBuildFlagsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(40, @"
        [SerializeField]
        private bool _enableFallback;
        public override bool EnableFallback { get => _enableFallback; set => _enableFallback = value; }

        [SerializeField]
        private SpawnableCrateReference _fallbackAsset;
        public override SpawnableCrateReference FallbackCrate
        {
            get => _fallbackAsset;
            set => _fallbackAsset = value;
        }
        "));
            return modRequests.ToArray();
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow/SLZ.Marrow.Warehouse/Scannable.cs")]
    public class ScannableBuildFlagsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(128, @"
        [SerializeField]
        private bool _packWithFlag = false;
        public bool PackWithFlag
        {
            get
            {
                return _packWithFlag;
            }
        }

        [SerializeField]
        private string _requiredBuildFlag;
        public string RequiredBuildFlag
        {
            get
            {
                return _requiredBuildFlag;
            }
        }"));
            return modRequests.ToArray();
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/ScannableEditor.cs")]
    public class ScannableEditorBuildFlagsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(17, new string[] {"protected SerializedProperty packWithFlagProperty;", "protected SerializedProperty requiredFlagProperty;"} ));
            modRequests.Add(new InsertLineRequest(30, new string[] {"packWithFlagProperty = serializedObject.FindProperty(\"_packWithFlag\");", "requiredFlagProperty = serializedObject.FindProperty(\"_requiredBuildFlag\");"}));
            modRequests.Add(new InsertLineRequest(58, new string[] {"LockedPropertyField(packWithFlagProperty, false);", "if(packWithFlagProperty.boolValue)", "{", "    LockedPropertyField(requiredFlagProperty, false);", "}"}));
            return modRequests.ToArray();
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/PalletEditor.cs")]
    public class PalletEditorBuildFlagsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(28, "SerializedProperty buildFlagsProperty;"));
            modRequests.Add(new InsertLineRequest(58, "buildFlagsProperty = serializedObject.FindProperty(\"_buildFlags\");"));
            modRequests.Add(new InsertLineRequest(343, @"
                if (GUILayout.Button(new GUIContent(""Prepare Pallet"", ""Prepare the Pallet in Addressables""), GUILayout.ExpandWidth(false)))
                {
                    PalletPackerEditor.PreparePallet(pallet, true, EditorPrefs.GetBool(""PackWithDedupe"", false));
                }
            "));
            modRequests.Add(new InsertLineRequest(404, "LockedPropertyField(buildFlagsProperty, false);"));
            return modRequests.ToArray();
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow/SLZ.Marrow.Warehouse/Pallet.cs")]
    public class PalletBuildFlagsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(155, @"
        [System.Serializable]
        public struct ToggleableBuildFlag
        {
            public string flag;
            public bool enabled;
        }

        [SerializeField]
        private List<ToggleableBuildFlag> _buildFlags = new List<ToggleableBuildFlag>();
        public List<string> BuildFlags
        {
            get
            {
                return _buildFlags.Where(b => b.enabled).Select((b) => b.flag).ToList();
            }
        }
        "));
            modRequests.Add(new ReplaceLineRequest(168, "foreach (Crate crate in Crates)"));
            modRequests.Add(new InsertLineRequest(169, @"{
                if (crate.PackWithFlag && !BuildFlags.Contains(crate.RequiredBuildFlag)) continue;
                scannables.Add(crate);
            }
            "));
            modRequests.Add(new ReplaceLineRequest(196, "if (crate == null) continue;"));
            modRequests.Add(new ReplaceLineRequest(197, "if (crate.PackWithFlag && !BuildFlags.Contains(crate.RequiredBuildFlag)) continue;"));
            modRequests.Add(new ReplaceLineRequest(198, "jsonCrateArray.Add(store.PackReference(crate));"));
            modRequests.Add(new DeleteLineRequest(199));
            return modRequests.ToArray();
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/PalletPackerEditor.cs")]
    public class PalletPackerEditorBuildFlagsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(23, "public static Pallet LastPackedPallet { get; private set; }\n"));
            modRequests.Add(new InsertLineRequest(33, "LastPackedPallet = pallet;"));
            modRequests.Add(new InsertLineRequest(108, "if (crate.ExcludeMainAsset) continue;"));
            modRequests.Add(new InsertLineRequest(241, "if (crate.ExcludeMainAsset) return;"));
            return modRequests.ToArray();
        }
    }
}