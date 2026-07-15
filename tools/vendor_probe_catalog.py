# Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
"""Descriptor-driven OPC Classic vendor scenario catalogs."""
from __future__ import annotations
import json, math, os, platform
from pathlib import Path
from typing import Any
ROOT=Path(__file__).resolve().parents[1]/'interop'/'tools'/'vendor-descriptors'
DESCRIPTORS=ROOT/'descriptors'
PROFILE_IDS={'matrikon':'matrikon-opc-simulation-server','testserver':'opc-foundation-testserver'}
class VendorDescriptorError(ValueError): pass

def _pairs(pairs):
 d={}
 for k,v in pairs:
  if k in d: raise VendorDescriptorError(f"Duplicate JSON property '{k}'.")
  d[k]=v
 return d
def _constant(v): raise VendorDescriptorError(f"Non-finite JSON number '{v}' is forbidden.")
def load_descriptor_json(text,source='<memory>'):
 try:d=json.loads(text,object_pairs_hook=_pairs,parse_constant=_constant)
 except (json.JSONDecodeError,VendorDescriptorError) as e: raise VendorDescriptorError(f"Invalid descriptor JSON in {source}: {e}") from e
 validate_descriptor(d,source); return d
def load_descriptor(profile_or_id):
 id=PROFILE_IDS.get(profile_or_id,profile_or_id); p=DESCRIPTORS/f'{id}.json'
 if not p.is_file(): raise FileNotFoundError(p)
 d=load_descriptor_json(p.read_text(encoding='utf-8'),str(p))
 if d['id']!=id: raise VendorDescriptorError('Descriptor id does not match file name.')
 return str(p),d
def validate_descriptor(d,source='<memory>'):
 errors=[]; required={'schemaVersion','id','vendor','product','target','capabilities','prerequisites','arguments','fixtures','probes','legal'}
 if not isinstance(d,dict): raise VendorDescriptorError('Descriptor must be an object.')
 if set(d)!=required: errors.append('Root properties do not match version 1 schema.')
 if d.get('schemaVersion')!='1.0': errors.append('Unsupported schema version.')
 caps=d.get('capabilities',[])
 if not isinstance(caps,list) or len(caps)!=len(set(caps)): errors.append('Capabilities must be unique.')
 fixture_ids=set()
 for i,f in enumerate(d.get('fixtures',[])):
  if not isinstance(f,dict) or set(f)!={'id','specification','variant','path','encoding','redistributable','expectedDecode'}: errors.append(f'fixtures[{i}] invalid.'); continue
  if f['id'] in fixture_ids: errors.append(f'fixtures[{i}] duplicate id.')
  fixture_ids.add(f['id'])
  if f['specification'] not in {'da','ae','hda'} or f['variant'] not in {'standard','empty','malformed','truncated','vendor-extension'}: errors.append(f'fixtures[{i}] invalid kind.')
  if f['encoding']!='hex' or f['redistributable'] is not True or f['expectedDecode'] not in {'success','failure'}: errors.append(f'fixtures[{i}] invalid decode contract.')
  if not isinstance(f['path'],str) or '..' in f['path'].split('/') or os.path.isabs(f['path']) or '\\' in f['path']: errors.append(f'fixtures[{i}] unsafe path.')
 probe_ids=set()
 for i,p in enumerate(d.get('probes',[])):
  if not isinstance(p,dict): errors.append(f'probes[{i}] invalid.'); continue
  if not {'id','type','requires','expected','expectedFailures'}<=set(p): errors.append(f'probes[{i}] missing fields.'); continue
  if p['id'] in probe_ids: errors.append(f'probes[{i}] duplicate id.')
  probe_ids.add(p['id'])
  if any(c not in caps for c in p['requires']): errors.append(f'probes[{i}] undeclared capability.')
  if p['type']=='fixture-decode' and p.get('fixtureId') not in fixture_ids: errors.append(f'probes[{i}] missing fixture.')
  if p['expected'].get('outcome') not in {'success','failure','skip'}: errors.append(f'probes[{i}] bad outcome.')
 forbidden={'password','secret','payload','binary','base64','command','script'}
 def safe(v,path='$'):
  if isinstance(v,dict):
   for k,x in v.items():
    if k.casefold() in forbidden: errors.append(f'{path}.{k} forbidden.')
    safe(x,f'{path}.{k}')
  elif isinstance(v,list):
   for j,x in enumerate(v): safe(x,f'{path}[{j}]')
  elif isinstance(v,float) and not math.isfinite(v): errors.append(f'{path} non-finite.')
 safe(d)
 da=d.get('arguments',{}).get('da')
 if da:
  lengths=[len(da.get(k,[])) for k in ('itemIds','clientHandles','writeValues')]
  if len(set(lengths))!=1 or not lengths[0]: errors.append('DA item, handle, and write arrays must align.')
 if errors: raise VendorDescriptorError(f"Descriptor '{source}' invalid:\n"+'\n'.join(errors))
def selected_catalog_probes(d):
 caps=set(d['capabilities']); return [p for p in d['probes'] if set(p['requires'])<=caps]
def selected_probe_scenarios(d): return [{'probeId':p['id'],'tool':p['tool']} for p in selected_catalog_probes(d) if 'tool' in p]
def selected_probe_tools(d): return sorted({'opcclassic.session.create','opcclassic.session.list','opcclassic.session.close',*(p['tool'] for p in selected_catalog_probes(d) if 'tool' in p)})
def require_finite_numbers(value,path='$'):
 if isinstance(value,float) and not math.isfinite(value): raise VendorDescriptorError(f'{path} is non-finite.')
 if isinstance(value,dict):
  for key,child in value.items(): require_finite_numbers(child,f'{path}.{key}')
 elif isinstance(value,list):
  for index,child in enumerate(value): require_finite_numbers(child,f'{path}[{index}]')
def final_probe_arguments(d):
 a=d['arguments']; require_finite_numbers(a,'$.arguments'); out=[]; da=a.get('da')
 if da:
  out += ['--da-browse-branch',da['browseBranch'],'--da-browse-filter',da['browseFilter'],'--da-read-item',da['itemIds'][0],'--da-group-name',da['groupName'],'--da-group-active',str(da['active']).lower(),'--da-update-rate-ms',str(da['updateRateMs']),'--da-read-from-cache',str(da['fromCache']).lower(),'--da-subscription-from-cache',str(da['fromCache']).lower(),'--da-max-notifications',str(da['maxNotifications'])]
  for value in da['itemIds']: out += ['--da-item-id',value]
  for value in da['clientHandles']: out += ['--da-client-handle',str(value)]
  for value in da['writeValues']:
   if isinstance(value,float) and not math.isfinite(value): raise VendorDescriptorError('Write value is non-finite.')
   out += ['--da-write-value-json',json.dumps(value,separators=(',',':'),allow_nan=False)]
 ae=a.get('ae')
 if ae: out += ['--ae-source',ae['source'],'--ae-condition',ae['condition']]
 h=a.get('hda')
 if h: out += ['--hda-item',h['itemId'],'--hda-start',h['startTime'],'--hda-end',h['endTime'],'--hda-at-time',h['atTime']]
 return out
def _hr(v):
 if isinstance(v,int): return f'0x{v&0xffffffff:08X}'
 if isinstance(v,str):
  try:return f'0x{int(v,0)&0xffffffff:08X}'
  except ValueError:return v.upper()
 return None
def classify_probe(p,success,error=None):
 if not success and error and any(f['code'].casefold() in error.casefold() for f in p['expectedFailures']): return 'BLOCKED'
 expected=p['expected']['outcome']
 if expected=='success': return 'MATCH' if success else 'REGRESSION'
 if expected=='failure': return 'UNEXPECTED_PASS' if success else 'MATCH'
 return 'UNEXPECTED_PASS' if success else 'NOT_APPLICABLE'
def evaluate_probe_result(p,row):
 success=bool(row.get('success')); actual={'outcome':'success' if success else 'failure','result':row.get('result'),'error':row.get('error')}; verdict=classify_probe(p,success,row.get('error'))
 if verdict!='MATCH' or not success:return verdict,actual
 expected=p['expected']; failures=[]; item=expected.get('itemId')
 if item is not None:
  values=row.get('result') if isinstance(row.get('result'),list) else [row.get('result')]
  match=next((x for x in values if isinstance(x,dict) and x.get('itemId',x.get('itemName'))==item),None); actual['itemResult']=match
  if match is None: failures.append(f"Expected item '{item}' was not returned.")
  elif 'hResult' in expected:
   actual['hResult']=_hr(match.get('hResult'))
   if actual['hResult']!=_hr(expected['hResult']): failures.append('hResult mismatch.')
 minimum=expected.get('minimumCount')
 if minimum is not None:
  result=row.get('result')
  count=len(result) if isinstance(result,(list,dict)) else 0
  actual['count']=count
  if count<minimum: failures.append(f'Expected at least {minimum} results, actual {count}.')
 if failures: actual['expectationFailures']=failures; return 'REGRESSION',actual
 return verdict,actual
def report_metadata(d): return {'descriptorVersion':d['schemaVersion'],'descriptorId':d['id'],'probeCatalogVersion':'1.0','vendor':d['vendor'],'product':d['product'],'targetKind':d['target']['kind'],'capabilityIds':list(d['capabilities']),'authMode':'operator-supplied','runnerOperatingSystem':platform.system().lower(),'runnerBitness':platform.architecture()[0]}
def external_prerequisite_results(d,roots):
 out=[]
 for p in d['prerequisites']:
  a=p.get('artifact')
  if not p['required'] or not a: continue
  root=roots.get(a['rootToken']); path=os.path.join(root,*a['relativePath'].split('/')) if root and os.path.isabs(root) else None; ok=bool(path and os.path.isfile(path))
  out.append({'probeId':p['id'],'expected':{'outcome':'success'},'actual':{'outcome':'success' if ok else 'blocked','path':path,'code':None if ok else ('FILE_NOT_FOUND' if path else 'INSTALL_ROOT_NOT_PROVIDED')},'verdict':'MATCH' if ok else 'BLOCKED',**report_metadata(d)})
 return out
def decode_fixture(d,id):
 f=next((x for x in d['fixtures'] if x['id']==id),None)
 if f is None: raise KeyError(id)
 path=(DESCRIPTORS/f['path']).resolve()
 if DESCRIPTORS.resolve() not in path.parents: raise VendorDescriptorError('Fixture path escapes catalog.')
 try:
  text=''.join(path.read_text(encoding='ascii').split())
  if len(text)%2: raise ValueError('Hex fixture contains a truncated byte.')
  data=bytes.fromhex(text); ok=True; error=None
 except (OSError,UnicodeError,ValueError) as e: data=b''; ok=False; error=str(e)
 expected=f['expectedDecode']=='success'
 return {'probeId':next((p['id'] for p in d['probes'] if p.get('fixtureId')==id),None),'expected':{'outcome':f['expectedDecode'],'specification':f['specification'],'variant':f['variant'],'redistributable':f['redistributable']},'actual':{'outcome':'success' if ok else 'failure','length':len(data),'error':error},'verdict':'MATCH' if ok==expected else 'REGRESSION',**report_metadata(d)}
