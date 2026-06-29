using System.Collections.Generic;
using EvroDev.FileModLib;
using SLZ.Marrow.Warehouse;

namespace LabWorks.ExtendedPalletPacking
{
    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow/SLZ.Marrow.Warehouse/Pallet.cs")]
    public class PalletEmbeddedPalletsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(155, @"
        [SerializeField]
        private List<Pallet> _embeddedPallets = new List<Pallet>();

        public List<Pallet> EmbeddedPallets
        {
            get
            {
                return _embeddedPallets;
            }
        }
        "));
            return modRequests.ToArray();
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/PalletEditor.cs")]
    public class PalletEditorEmbeddedPalletsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(28, "SerializedProperty embeddedPalletsProperty;"));
            modRequests.Add(new InsertLineRequest(58, "embeddedPalletsProperty = serializedObject.FindProperty(\"_embeddedPallets\");"));
            modRequests.Add(new InsertLineRequest(404, "LockedPropertyField(embeddedPalletsProperty, false);"));
            return modRequests.ToArray();
        }
    }

    [FileModifier("Packages/com.stresslevelzero.marrow.sdk.extended/Scripts/SLZ.Marrow.Editor/SLZ.MarrowEditor/PalletPackerEditor.cs")]
    public class PalletPackerEditorEmbeddedPalletsModifier : IFileModRequester
    {
        public IFileModRequest[] OnModifyFile()
        {
            List<IFileModRequest> modRequests = new();
            modRequests.Add(new InsertLineRequest(14, "using System.IO;"));
            modRequests.Add(new InsertLineRequest(36, "ApplyEmbededDummyCatalogs(pallet);"));
            modRequests.Add(new InsertLineRequest(39, @"
        public static void ApplyEmbededDummyCatalogs(Pallet pallet)
        {
            string CATALOG_CONTENT = ""{\""m_LocatorId\"":\""AddressablesMainContentCatalog\"",\""m_BuildResultHash\"":\""00000000000000000000000000000000\"",\""m_InstanceProviderData\"":{\""m_Id\"":\""UnityEngine.ResourceManagement.ResourceProviders.InstanceProvider\"",\""m_ObjectType\"":{\""m_AssemblyName\"":\""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\"",\""m_ClassName\"":\""UnityEngine.ResourceManagement.ResourceProviders.InstanceProvider\""},\""m_Data\"":\""\""},\""m_SceneProviderData\"":{\""m_Id\"":\""UnityEngine.ResourceManagement.ResourceProviders.SceneProvider\"",\""m_ObjectType\"":{\""m_AssemblyName\"":\""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\"",\""m_ClassName\"":\""UnityEngine.ResourceManagement.ResourceProviders.SceneProvider\""},\""m_Data\"":\""\""},\""m_ResourceProviderData\"":[],\""m_ProviderIds\"":[],\""m_InternalIds\"":[],\""m_KeyDataString\"":\""AAAAAA==\"",\""m_BucketDataString\"":\""AAAAAA==\"",\""m_EntryDataString\"":\""AAAAAA==\"",\""m_ExtraDataString\"":\""\"",\""m_resourceTypes\"":[],\""m_InternalIdPrefixes\"":[]}"";
            string CATALOG_HASH = ""019cbabadf9fdfbbfed0a1bfd0c8ef56"";

            string basePath = AddressablesManager.GetBuiltModFolder(pallet);

            foreach (Pallet embed in pallet.EmbeddedPallets)
            {
                string catalogPath = Path.Combine(basePath, $""catalog_{embed.Barcode.ID}.json"");
                string hashPath = Path.Combine(basePath, $""catalog_{embed.Barcode.ID}.hash"");
                string palletPath = Path.Combine(basePath, $""{embed.Barcode.ID}.pallet.json"");

                File.WriteAllText(catalogPath, CATALOG_CONTENT);
                File.WriteAllText(hashPath, CATALOG_HASH);

                PalletPacker.PackAndSaveToJson(embed, palletPath);
            }
        }
            "));
            modRequests.Add(new InsertLineRequest(95, @"
            foreach (Pallet embeddedPallet in pallet.EmbeddedPallets)
            {
                if (generatePackedAssets)
                    GeneratePackedAssets(embeddedPallet);
                PreparePalletInternal(settings, embeddedPallet, pallet);
            }
            "));
            modRequests.Add(new ReplaceLineRequest(139, "private static Dictionary<Type, AddressableAssetGroup> PreparePalletInternal(AddressableAssetSettings settings, Pallet pallet, Pallet parentPallet = null)"));
            modRequests.Add(new ReplaceLineRequest(169, "AddScannableAssetsToGroup(scannable, settings, pallet, scannableTypeToGroup, parentPallet);"));
            modRequests.Add(new ReplaceLineRequest(197, "private static AddressableAssetGroup SetupPalletGroup(AddressableAssetSettings settings, Pallet pallet, Type scannableType, Pallet palletBuildPathOverride = null)"));
            modRequests.Add(new InsertLineRequest(213, "Pallet palletForBuildPath = palletBuildPathOverride != null ? palletBuildPathOverride : pallet;"));
            modRequests.Add(new ReplaceLineRequest(213, "string buildPath = AddressablesManager.EvaluateProfileValueBuildPathForPallet(palletForBuildPath, profileID);"));
            modRequests.Add(new ReplaceLineRequest(214, "string loadPath = AddressablesManager.EvaluateProfileValueLoadPathForPallet(palletForBuildPath, AddressablesManager.ProfilePalletID);"));
            modRequests.Add(new ReplaceLineRequest(225, "private static void AddScannableAssetsToGroup(Scannable scannable, AddressableAssetSettings settings, Pallet pallet, Dictionary<Type, AddressableAssetGroup> scannableTypeToGroup, Pallet palletBuildPathOverride = null)"));
            modRequests.Add(new ReplaceLineRequest(232, "scannableTypeToGroup.Add(scannableType, SetupPalletGroup(settings, pallet, scannableType, palletBuildPathOverride));"));
            return modRequests.ToArray();
        }
    }
}