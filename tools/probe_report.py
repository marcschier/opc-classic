import json, sys
d = json.load(open(sys.argv[1], encoding='utf-8'))
ok = sum(1 for r in d if r.get('success'))
print(f'total={len(d)} ok={ok} fail={len(d)-ok}')
print()
for r in d:
    tag = 'OK  ' if r['success'] else 'FAIL'
    line = r.get('summary','') if r['success'] else (r.get('error','') or '')
    line = line.replace('\n',' ').replace('\r',' ')[:120]
    print(f"{tag} {r['tool']:50} {line}")
