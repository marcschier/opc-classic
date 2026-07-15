# Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
from __future__ import annotations
import json, math, os, shutil, sys, unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch
HERE=Path(__file__).resolve().parent; REPO=HERE.parent
if str(HERE) not in sys.path: sys.path.insert(0,str(HERE))
import probe_servers, run_cross_impl_matrix, vendor_probe_catalog as v
class VendorProbeCatalogTests(unittest.TestCase):
 def setUp(self): self.scratch=REPO/'artifacts'/f'vendor-probe-{os.getpid()}'; self.scratch.mkdir(parents=True,exist_ok=True)
 def tearDown(self): shutil.rmtree(self.scratch,ignore_errors=True)
 def test_catalog_selection_and_coverage(self):
  _,d=v.load_descriptor('generic-opc-classic-template'); ids={p['id'] for p in v.selected_catalog_probes(d)}; types={p['type'] for p in d['probes']}; tools=v.selected_probe_tools(d)
  self.assertTrue({'da-optional-interface-query','ae-condition-state','hda-relative-time'}<=ids)
  self.assertTrue({'da-query-interface','da-group-lifecycle','da-callback','da-deadband','da-sampling','da-browse','da-properties','da-sync-read','da-sync-write','da-async-read','da-async-write','da-reconnect','da-failover','ae-subscription','ae-filter','ae-returned-attributes','ae-refresh','ae-cancel-refresh','ae-condition-state','hda-browser','hda-read-raw','hda-read-processed','hda-read-modified','hda-annotations','hda-advise','hda-playback','hda-aggregates','hda-relative-time','fixture-decode'}<=types)
  self.assertIn('opcclassic.ae.refresh_subscription',tools); self.assertIn('opcclassic.hda.read_modified',tools)
  reduced=json.loads(json.dumps(d)); reduced['capabilities'].remove('ae-condition-state'); self.assertNotIn('ae-condition-state',{p['id'] for p in v.selected_catalog_probes(reduced)})
 def test_final_arguments_reach_parser(self):
  _,d=v.load_descriptor('generic-opc-classic-template'); argv=v.final_probe_arguments(d)
  with patch.object(sys,'argv',['probe_servers.py','--da-progid',d['target']['progid'],*argv]): a=probe_servers.parse_args()
  self.assertEqual(a.da_read_item,'Vendor.Writable'); self.assertEqual(a.da_item_ids,['Vendor.Writable']); self.assertEqual(a.da_write_values,[1]); self.assertEqual(a.ae_source,'Vendor.Source'); self.assertEqual(a.hda_item,'Vendor.History'); self.assertEqual(a.hda_start,'NOW-1H')
 def test_verdicts_block_external_failures(self):
  _,d=v.load_descriptor('matrikon'); p=next(x for x in d['probes'] if x['id']=='da-reconnect')
  self.assertEqual(v.classify_probe(p,False,'DCOM E_ACCESSDENIED'),'BLOCKED'); self.assertEqual(v.classify_probe(p,False,'decode failed'),'REGRESSION'); self.assertEqual(v.classify_probe(p,True),'MATCH')
 def test_fixture_decode_and_metadata(self):
  _,d=v.load_descriptor('generic-opc-classic-template'); malformed=v.decode_fixture(d,'da-malformed'); truncated=v.decode_fixture(d,'da-truncated'); extension=v.decode_fixture(d,'da-vendor-extension'); ae=v.decode_fixture(d,'ae-condition-state-vendor-extension')
  self.assertEqual(malformed['verdict'],'MATCH'); self.assertEqual(malformed['actual']['outcome'],'failure'); self.assertIn('non-hexadecimal',malformed['actual']['error']); self.assertIn('truncated',truncated['actual']['error']); self.assertEqual(extension['actual']['length'],6); self.assertEqual(extension['verdict'],'MATCH'); self.assertEqual(ae['descriptorId'],'generic-opc-classic-template'); self.assertEqual(ae['probeCatalogVersion'],'1.0'); self.assertEqual(ae['expected']['variant'],'vendor-extension')
 def test_external_artifacts_are_blocked(self):
  _,d=v.load_descriptor('testserver'); unresolved=v.external_prerequisite_results(d,{}); missing=v.external_prerequisite_results(d,{'OPC_TESTSERVER_INSTALL_ROOT':str(self.scratch.resolve())})
  self.assertEqual(unresolved[0]['actual']['code'],'INSTALL_ROOT_NOT_PROVIDED'); self.assertEqual(unresolved[0]['verdict'],'BLOCKED'); self.assertEqual(missing[0]['actual']['code'],'FILE_NOT_FOUND'); self.assertEqual(missing[0]['verdict'],'BLOCKED')
 def test_matrix_blocks_before_external_execution(self):
  args=__import__('argparse').Namespace(use_clsid=False,host='localhost',request_timeout=60.0,username=None,password=None,use_kerberos=False,auth_level=None,save_wire_payloads=None)
  with patch.dict(os.environ,{},clear=True), patch.object(run_cross_impl_matrix.subprocess,'run') as execute: result=run_cross_impl_matrix.run_profile(args,'testserver',{})
  self.assertTrue(result['skipped']); self.assertEqual(result['raw_results'][0]['verdict'],'BLOCKED'); execute.assert_not_called()
 def test_multiple_same_tool_scenarios_are_preserved(self):
  d={'capabilities':['sync-write'],'probes':[{'id':'one','requires':['sync-write'],'tool':'opcclassic.da.write_sync'},{'id':'two','requires':['sync-write'],'tool':'opcclassic.da.write_sync'}]}
  self.assertEqual(v.selected_probe_scenarios(d),[{'probeId':'one','tool':'opcclassic.da.write_sync'},{'probeId':'two','tool':'opcclassic.da.write_sync'}]); self.assertEqual(v.selected_probe_tools(d).count('opcclassic.da.write_sync'),1)
  class Client:
   def __init__(self): self.calls=[]
   def call_tool(self,name,arguments): self.calls.append((name,arguments)); return [{'itemId':'Vendor.Writable','hResult':0}]
  client=Client(); args=SimpleNamespace(probe=None,probe_scenarios=[('one','opcclassic.da.write_sync'),('two','opcclassic.da.write_sync')],da_write_values=[1]); runner=probe_servers.ProbeRunner(args,client)
  rows=runner.run([{'name':'opcclassic.da.write_sync'}])
  self.assertEqual([row['probeId'] for row in rows],['one','two']); self.assertEqual(len(client.calls),2)
 def test_expected_item_is_matched_not_first_result(self):
  _,descriptor=v.load_descriptor('testserver'); declared=next(x for x in descriptor['probes'] if x['id']=='da-sync-write'); self.assertEqual(declared['expected']['itemId'],'Test.Int32')
  p={'expected':{'outcome':'success','itemId':'Second.Item','hResult':'0x00000000'},'expectedFailures':[]}; row={'success':True,'result':[{'itemId':'First.Item','hResult':0x80004005},{'itemId':'Second.Item','hResult':0}]}; verdict,actual=v.evaluate_probe_result(p,row)
  self.assertEqual(verdict,'MATCH'); self.assertEqual(actual['itemResult']['itemId'],'Second.Item'); self.assertEqual(actual['hResult'],'0x00000000')
 def test_missing_expected_item_is_regression(self):
  p={'expected':{'outcome':'success','itemId':'Missing.Item','hResult':'0x00000000'},'expectedFailures':[]}; verdict,actual=v.evaluate_probe_result(p,{'success':True,'result':[{'itemId':'Other.Item','hResult':0}]}); self.assertEqual(verdict,'REGRESSION'); self.assertIn('Missing.Item',actual['expectationFailures'][0])
 def test_loader_and_final_args_reject_nonfinite(self):
  for c in ('NaN','Infinity','-Infinity'):
   with self.assertRaisesRegex(v.VendorDescriptorError,'Non-finite'): v.load_descriptor_json('{"value":'+c+'}')
  _,d=v.load_descriptor('generic-opc-classic-template'); d=json.loads(json.dumps(d)); d['arguments']['da']['writeValues'][0]=math.nan
  with self.assertRaisesRegex(v.VendorDescriptorError,'non-finite'): v.final_probe_arguments(d)
  _,d=v.load_descriptor('generic-opc-classic-template'); d['probes'][0]['expected']['minimumCount']=math.inf
  with self.assertRaisesRegex(v.VendorDescriptorError,'non-finite'): v.validate_descriptor(d)
  with self.assertRaisesRegex(ValueError,'non-finite'): probe_servers.require_finite_numbers({'value':math.inf},'$')
 def test_generic_catalog_has_no_product_binaries(self):
  _,d=v.load_descriptor('generic-opc-classic-template'); self.assertEqual(d['vendor'],'Operator supplied vendor'); self.assertFalse(any(p.suffix.lower() in {'.exe','.dll','.msi'} for p in v.DESCRIPTORS.rglob('*')))
if __name__=='__main__': unittest.main(verbosity=2)
