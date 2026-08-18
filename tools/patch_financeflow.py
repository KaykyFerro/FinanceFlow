from pathlib import Path
import re

path = Path('index.html')
s = path.read_text(encoding='utf-8')

marker = '.tablewrap{overflow:auto}'
css = '''.txfilters{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin-bottom:18px;padding:14px;border:1px solid var(--line);border-radius:10px;background:var(--soft)}.txfilter{display:grid;gap:6px}.txfilter label{font-size:11px;font-weight:800;color:var(--muted)}.txfilter input,.txfilter select{width:100%;border:1px solid var(--line);background:var(--input);color:var(--text);padding:10px 11px;border-radius:8px;outline:0}.txfilter input:focus,.txfilter select:focus{border-color:var(--green)}.txfilter-actions{display:flex;align-items:end}.txfilter-actions .btn{width:100%}.txcount{font-size:12px;color:var(--muted)}\n'''
if '.txfilters{' not in s:
    s = s.replace(marker, css + marker, 1)

s = s.replace("const state={view:'dashboard',date:new Date(),data:null};", "const state={view:'dashboard',date:new Date(),data:null,transactionFilters:{from:'',to:'',category:'',type:''}};", 1)

pattern = re.compile(r"function transactions\(el\)\{.*?\}\nfunction reports", re.S)
new_transactions = r'''function transactions(el){
const d=state.data;
const all=d.allTransactions||d.transactions||[];
const f=state.transactionFilters;
const filtered=all.filter(t=>{
  const day=new Date(t.date).toISOString().slice(0,10);
  if(f.from && day<f.from)return false;
  if(f.to && day>f.to)return false;
  if(f.category && t.categoryId!==f.category)return false;
  if(f.type && t.type!==f.type)return false;
  return true;
});
const categoryOptions=d.categories.map(c=>`<option value="${c.id}" ${f.category===c.id?'selected':''}>${esc(c.name)}</option>`).join('');
el.innerHTML=head('Transações','Registre e acompanhe todas as movimentações',`<button class="btn" data-action="transaction">＋ Nova Transação</button>`)+`<div class="panel" style="margin-top:24px"><div class="panelhead"><div><h2>${filtered.length} movimentação(ões)</h2><span class="txcount">${all.length} cadastrada(s) no total</span></div><span class="muted">Todas as datas</span></div><div class="txfilters"><div class="txfilter"><label>Data inicial</label><input type="date" data-filter="from" value="${f.from}"></div><div class="txfilter"><label>Data final</label><input type="date" data-filter="to" value="${f.to}"></div><div class="txfilter"><label>Categoria</label><select data-filter="category"><option value="">Todas as categorias</option>${categoryOptions}</select></div><div class="txfilter"><label>Tipo</label><select data-filter="type"><option value="" ${!f.type?'selected':''}>Todos</option><option value="Income" ${f.type==='Income'?'selected':''}>Entradas</option><option value="Expense" ${f.type==='Expense'?'selected':''}>Saídas</option></select></div><div class="txfilter-actions"><button class="btn secondary" data-clear-tx-filters>Limpar filtros</button></div></div><div class="tablewrap"><table class="table"><thead><tr><th>Data</th><th>Descrição</th><th>Categoria</th><th>Conta</th><th>Tipo</th><th>Valor</th><th>Ações</th></tr></thead><tbody>${filtered.length?filtered.map(t=>{const c=d.categories.find(x=>x.id===t.categoryId),a=d.accounts.find(x=>x.id===t.accountId);return `<tr><td>${dateBR(t.date)}</td><td><b>${esc(t.description)}</b></td><td>${c?esc(c.name):'Sem categoria'}</td><td>${a?esc(a.name):'-'}</td><td><span class="pill ${t.type==='Income'?'in':'out'}">${t.type==='Income'?'Entrada':'Saída'}</span></td><td class="${t.type==='Income'?'green':'red'}"><b>${money(t.amount)}</b></td><td><div class="txactions"><button class="iconbtn edit-tx-btn" data-edit-tx="${t.id}" title="Editar transação" aria-label="Editar transação">✎ <span>Editar</span></button><button class="iconbtn" data-delete-tx="${t.id}" title="Excluir transação" aria-label="Excluir transação">🗑</button></div></td></tr>`}).join(''):`<tr><td colspan="7" class="muted">Nenhuma transação encontrada com os filtros selecionados.</td></tr>`}</tbody></table></div></div>`}
function reports'''
if not pattern.search(s):
    raise SystemExit('transactions function not found')
s = pattern.sub(new_transactions, s, count=1)

old = "const dt=e.target.closest('[data-delete-tx]');if(dt){remove('/finance/transactions/'+dt.dataset.deleteTx,'Transação excluída.');return;}"
new = "const clear=e.target.closest('[data-clear-tx-filters]');if(clear){state.transactionFilters={from:'',to:'',category:'',type:''};render();return}const dt=e.target.closest('[data-delete-tx]');if(dt){remove('/finance/transactions/'+dt.dataset.deleteTx,'Transação excluída.');return;}"
if old not in s:
    raise SystemExit('delete listener marker not found')
s = s.replace(old, new, 1)

marker = "document.addEventListener('change',e=>{if(e.target.id==='themeSelect')setTheme(e.target.value)});"
replacement = "document.addEventListener('change',e=>{if(e.target.id==='themeSelect')setTheme(e.target.value);const f=e.target.closest('[data-filter]');if(f){state.transactionFilters[f.dataset.filter]=f.value;render()}});document.addEventListener('input',e=>{const f=e.target.closest('[data-filter]');if(f && f.type==='date'){state.transactionFilters[f.dataset.filter]=f.value;render()}});"
if marker not in s:
    raise SystemExit('change listener marker not found')
s = s.replace(marker, replacement, 1)

path.write_text(s, encoding='utf-8')
print('patched', path)
