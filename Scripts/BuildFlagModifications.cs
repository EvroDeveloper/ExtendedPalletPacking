using System.Collections.Generic;
using EvroDev.FileModLib;
using SLZ.Marrow.Warehouse;

namespace LabWorks.ExtendedPalletPacking
{
    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/CrateEditor.cs")]
    public class CrateEditorBuildFlagsModifier : FileModRequester
    {
        public override void OnModifyFile()
        {
            Request(new InsertLineRequest(33, "SerializedProperty excludeAssetsProperty;"));
            Request(new InsertLineRequest(57, "excludeAssetsProperty = serializedObject.FindProperty(\"_excludeAssets\");"));
            Request(new InsertLineRequest(130, "EditorGUILayout.PropertyField(excludeAssetsProperty);"));
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow/SLZ.Marrow.Warehouse/Crate.cs")]
    public class CrateBuildFlagsModifier : FileModRequester
    {
        public override void OnModifyFile()
        {
            Request(new InsertLineRequest(21, @"
        public static bool UseFallbackGuid { get; set; }
        public virtual bool EnableFallback { get; set; }
        public virtual SpawnableCrateReference FallbackCrate { get; set; }
        [SerializeField]
        private bool _excludeAssets;
        public virtual bool ExcludeMainAsset { get => _excludeAssets; set => _excludeAssets = value; }"));
            Request(new DeleteLineRequest(103));
            Request(new InsertLineRequest(103, @"
            if (UseFallbackGuid && EnableFallback)
            {
                json.Add(""mainAsset"", FallbackCrate.Crate.MainAsset.AssetGUID);
            }
            else
            {
                json.Add(""mainAsset"", MainAsset.AssetGUID);
            }
            "));
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow/SLZ.Marrow.Warehouse/GameObjectCrate.cs")]
    public class GameObjectCrateBuildFlagsModifier : FileModRequester
    {
        public override void OnModifyFile()
        {
            Request(new InsertLineRequest(40, @"
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
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow/SLZ.Marrow.Warehouse/Scannable.cs")]
    public class ScannableBuildFlagsModifier : FileModRequester
    {
        public override void OnModifyFile()
        {
            Request(new InsertLineRequest(128, @"
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
        [Tooltip(""All build flags must be enabled in order for this scannable to pack"")]
        private string[] _requiredBuildFlags = new string[0];
        public string[] RequiredBuildFlags
        {
            get
            {
                return _requiredBuildFlags;
            }
        }"));
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/ScannableEditor.cs")]
    public class ScannableEditorBuildFlagsModifier : FileModRequester
    {
        public override void OnModifyFile()
        {
            Request(new InsertLineRequest(17, new string[] {"protected SerializedProperty packWithFlagProperty;", "protected SerializedProperty requiredFlagsProperty;"} ));
            Request(new InsertLineRequest(30, new string[] {"packWithFlagProperty = serializedObject.FindProperty(\"_packWithFlag\");", "requiredFlagsProperty = serializedObject.FindProperty(\"_requiredBuildFlags\");"}));
            Request(new InsertLineRequest(58, new string[] {"LockedPropertyField(packWithFlagProperty, false);", "if(packWithFlagProperty.boolValue)", "{", "    LockedPropertyField(requiredFlagsProperty, false);", "}"}));
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/PalletEditor.cs")]
    public class PalletEditorBuildFlagsModifier : FileModRequester
    {
        public override void OnModifyFile()
        {
            Request(new InsertLineRequest(28, "SerializedProperty buildFlagsProperty;"));
            Request(new InsertLineRequest(58, "buildFlagsProperty = serializedObject.FindProperty(\"_buildFlags\");"));
            Request(new InsertLineRequest(343, @"
                if (GUILayout.Button(new GUIContent(""Prepare Pallet"", ""Prepare the Pallet in Addressables""), GUILayout.ExpandWidth(false)))
                {
                    PalletPackerEditor.PreparePallet(pallet, true, EditorPrefs.GetBool(""PackWithDedupe"", false));
                }
            "));
            Request(new InsertLineRequest(404, "LockedPropertyField(buildFlagsProperty, false);"));
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow/SLZ.Marrow.Warehouse/Pallet.cs")]
    public class PalletBuildFlagsModifier : FileModRequester
    {
        public override void OnModifyFile()
        {
            Request(new InsertLineRequest(155, @"
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

        private bool CanBuildCrate(Crate crate)
        {
            if(crate == null) return false;
            if(!crate.PackWithFlag) return true;
            bool anyDisabled = false;
            foreach(string buildFlag in crate.RequiredBuildFlags)
            {
                if(!BuildFlags.Contains(buildFlag))
                {
                    anyDisabled = true;
                    break;
                }
            }
            return !anyDisabled;
        }
        "));
            Request(new ReplaceLineRequest(168, "foreach (Crate crate in Crates)"));
            Request(new InsertLineRequest(169, @"{
                if (CanBuildCrate(crate)) scannables.Add(crate);
            }
            "));
            Request(new ReplaceLineRequest(196, "if (crate == null) continue;"));
            Request(new ReplaceLineRequest(197, "if (!CanBuildCrate(crate)) continue;"));
            Request(new ReplaceLineRequest(198, "jsonCrateArray.Add(store.PackReference(crate));"));
            Request(new DeleteLineRequest(199));
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/PalletPackerEditor.cs")]
    public class PalletPackerEditorBuildFlagsModifier : FileModRequester
    {
        public override void OnModifyFile()
        {
            Request(new InsertLineRequest(23, "public static Pallet LastPackedPallet { get; private set; }\n"));
            Request(new InsertLineRequest(33, "LastPackedPallet = pallet;"));
            Request(new InsertLineRequest(108, "if (crate.ExcludeMainAsset) continue;"));
            Request(new InsertLineRequest(241, "if (crate.ExcludeMainAsset) return;"));
        }
    }
}