import React from 'react';

type BranchRecord = {
  branch: string;
  pressure: string;
  detail: string;
  tone: 'urgent' | 'high' | 'mid';
};

type NoticeItem = {
  title: string;
  summary: string;
  tier: 'urgent' | 'consequential' | 'background';
};

type VisitorItem = {
  title: string;
  summary: string;
};

type ReceiptItem = {
  title: string;
  summary: string;
};

type CountyNode = {
  name: string;
  status: string;
  left: string;
  top: string;
  active?: boolean;
};

const branchRecords: BranchRecord[] = [
  {
    branch: '东二房',
    pressure: '承祧未定',
    detail: '长支无子，旁支皆欲争名分。',
    tone: 'urgent',
  },
  {
    branch: '西三房',
    pressure: '婚议相持',
    detail: '聘财轻重不齐，女方仍未回帖。',
    tone: 'high',
  },
  {
    branch: '北支',
    pressure: '门内举哀未毕',
    detail: '丧次未整，长幼礼序仍有争言。',
    tone: 'high',
  },
  {
    branch: '南偏房',
    pressure: '借粮未清',
    detail: '秋收未至，月末仍待周转。',
    tone: 'mid',
  },
];

const visitors: VisitorItem[] = [
  {
    title: '族老候见',
    summary: '请先议承祧，勿使支房各执一词。',
  },
  {
    title: '账房递册',
    summary: '公中尚可先拨一仓粮、银三十两。',
  },
  {
    title: '里正来报',
    summary: '县门榜示方出，街谈已先于正信流入坊巷。',
  },
];

const notices: NoticeItem[] = [
  {
    title: '二房请定承祧',
    summary: '无子之后谁续香火，已牵动两支旧怨。',
    tier: 'urgent',
  },
  {
    title: '新婴体弱',
    summary: '若缓护养，来月恐生更重后果。',
    tier: 'urgent',
  },
  {
    title: '婚帖再缓',
    summary: '两家礼数尚可挽回，但不宜再拖。',
    tier: 'consequential',
  },
  {
    title: '县门拥堵',
    summary: '榜示、讼户、商旅并集，路报已显迟滞。',
    tier: 'consequential',
  },
  {
    title: '西市米价微涨',
    summary: '比上月略高，尚未成灾，但需早看。',
    tier: 'background',
  },
  {
    title: '渡口今晨稍缓',
    summary: '船只无争，城南往来较昨日顺畅。',
    tier: 'background',
  },
];

const receipts: ReceiptItem[] = [
  {
    title: '上月回执',
    summary: '已遣族老调停，三房暂罢恶语。',
  },
  {
    title: '账册记要',
    summary: '若先护婴，再议婚帖，公中仍可承受。',
  },
];

const countyNodes: CountyNode[] = [
  { name: '宗宅', status: '主位', left: '14%', top: '60%' },
  { name: '县门', status: '拥堵', left: '35%', top: '32%', active: true },
  { name: '西市', status: '价浮', left: '53%', top: '54%' },
  { name: '渡口', status: '缓行', left: '72%', top: '36%' },
  { name: '学塾', status: '催束脩', left: '80%', top: '66%' },
];

const commandLabels = ['议定承祧', '拨粮护婴', '遣族老调停', '缓议婚帖'];

const toneClasses: Record<BranchRecord['tone'], string> = {
  urgent: 'border-[#b35a45] bg-[#3b1d17] text-[#efc6b8]',
  high: 'border-[#9b7648] bg-[#34261a] text-[#e8cb98]',
  mid: 'border-[#617058] bg-[#1f261e] text-[#bfd0b1]',
};

function SectionFrame({
  title,
  subtitle,
  children,
  className = '',
}: {
  title: string;
  subtitle?: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <section className={`relative ${className}`}>
      <div className="absolute inset-0 translate-x-2 translate-y-2 bg-black/20" />
      <div className="relative border border-[#705438] bg-[#1b130e]/92 p-4 shadow-[0_12px_24px_rgba(0,0,0,0.28)]">
        <div className="text-[11px] tracking-[0.35em] text-[#a88964]">{title}</div>
        {subtitle ? <div className="mt-1 text-sm text-[#d7c4aa]">{subtitle}</div> : null}
        <div className="mt-4">{children}</div>
      </div>
    </section>
  );
}

function FamilyLedger() {
  return (
    <SectionFrame title="宗房簿" subtitle="看支房张力，不看全局面板">
      <div className="space-y-3">
        {branchRecords.map((item) => (
          <div key={item.branch} className={`border px-3 py-3 ${toneClasses[item.tone]}`}>
            <div className="flex items-center justify-between text-xs tracking-[0.18em]">
              <span>{item.branch}</span>
              <span className="opacity-75">支房</span>
            </div>
            <div className="mt-2 text-sm">{item.pressure}</div>
            <div className="mt-1 text-xs leading-5 opacity-80">{item.detail}</div>
          </div>
        ))}
      </div>
    </SectionFrame>
  );
}

function VisitorQueue() {
  return (
    <SectionFrame title="来人位" subtitle="先见谁，本身就是决策">
      <div className="space-y-3">
        {visitors.map((item) => (
          <div key={item.title} className="border-b border-[#4f3a27] pb-3 last:border-b-0 last:pb-0">
            <div className="text-sm text-[#ead8c1]">{item.title}</div>
            <div className="mt-1 text-xs leading-5 text-[#a88c69]">{item.summary}</div>
          </div>
        ))}
      </div>
    </SectionFrame>
  );
}

function CommandSeal({ label, primary = false }: { label: string; primary?: boolean }) {
  return (
    <button
      className={`group flex min-w-[126px] flex-col items-center justify-center rounded-full border px-4 py-3 shadow-[0_8px_18px_rgba(0,0,0,0.35)] transition hover:-translate-y-0.5 ${
        primary
          ? 'border-[#ba8662] bg-[linear-gradient(180deg,#7b3022,#5c2218)] text-[#f4e1cf]'
          : 'border-[#8e6a46] bg-[linear-gradient(180deg,#4c3827,#352518)] text-[#dec8a7]'
      }`}
    >
      <span className="text-[10px] tracking-[0.3em] opacity-75">可议</span>
      <span className="mt-1 text-sm tracking-[0.14em]">{label}</span>
    </button>
  );
}

function GreatHallLead() {
  return (
    <section className="relative overflow-hidden border border-[#7b5c39] bg-[linear-gradient(180deg,rgba(57,38,24,0.96),rgba(31,21,15,0.98))] px-6 py-6 shadow-[0_18px_34px_rgba(0,0,0,0.34)]">
      <div className="absolute left-0 right-0 top-0 h-10 bg-[linear-gradient(180deg,rgba(118,85,48,0.22),transparent)]" />

      <div className="mb-5 flex items-start justify-between gap-4">
        <div>
          <div className="text-[11px] tracking-[0.4em] text-[#b4936d]">堂上急报</div>
          <h1 className="mt-2 text-3xl tracking-[0.14em] text-[#f1e3cb]">先定承祧，再缓婚议</h1>
          <div className="mt-2 text-sm text-[#c9b291]">主焦点不是战争，不是任务，而是门内名分与抚护次序。</div>
        </div>

        <div className="rounded-sm border border-[#7f6140] bg-[#271a11] px-3 py-2 text-right">
          <div className="text-[10px] tracking-[0.32em] text-[#a18261]">本月主位</div>
          <div className="mt-1 text-sm text-[#ead6b6]">家族秩序 / 名分 / 抚恤</div>
        </div>
      </div>

      <div className="grid grid-cols-[minmax(0,1fr)_240px] gap-5">
        <div className="relative">
          <div className="absolute inset-0 translate-x-3 translate-y-3 border border-[#3f2b1d] bg-black/20" />
          <div className="relative border border-[#8d6c45] bg-[#efe0c7] px-6 py-5 text-[#312114] shadow-[0_12px_28px_rgba(0,0,0,0.22)]">
            <div className="text-[11px] tracking-[0.3em] text-[#886645]">堂批</div>
            <p className="mt-3 text-base leading-8">
              二房无子，请从旁支续后。三房又忧嫁资外流，不肯先退一步。门内新婴体弱，若只争名分而不先抚护，
              旧怨会趁机再起。
            </p>

            <div className="mt-4 flex flex-wrap gap-2">
              {['承祧未定', '婚议相持', '新婴待护', '旧怨未散'].map((tag) => (
                <span
                  key={tag}
                  className="border border-[#b78b5a] bg-[#f5ead7] px-2.5 py-1 text-xs tracking-[0.12em] text-[#6b4c2d]"
                >
                  {tag}
                </span>
              ))}
            </div>

            <div className="mt-5 border-t border-[#c8b08f] pt-4">
              <div className="text-[11px] tracking-[0.3em] text-[#896845]">眼下宜先</div>
              <div className="mt-2 text-lg tracking-[0.08em] text-[#5c3d22]">
                议定承祧，拨粮护婴；婚帖可缓，不可久拖。
              </div>
            </div>
          </div>
        </div>

        <div className="flex flex-col gap-3">
          <div className="border border-[#674b31] bg-[#22170f] px-4 py-4">
            <div className="text-[11px] tracking-[0.32em] text-[#a78863]">可议之事</div>
            <div className="mt-4 flex flex-wrap gap-3">
              {commandLabels.map((label, index) => (
                <CommandSeal key={label} label={label} primary={index < 2} />
              ))}
            </div>
          </div>

          <div className="border border-[#674b31] bg-[#22170f] px-4 py-4">
            <div className="text-[11px] tracking-[0.32em] text-[#a78863]">近回执</div>
            <div className="mt-3 space-y-3">
              {receipts.map((item) => (
                <div key={item.title} className="border-l border-[#8a6843] pl-3">
                  <div className="text-sm text-[#ead9bf]">{item.title}</div>
                  <div className="mt-1 text-xs leading-5 text-[#a58a69]">{item.summary}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

function NoticeCard({ item }: { item: NoticeItem }) {
  const accentClass =
    item.tier === 'urgent'
      ? 'border-[#b65b45] before:bg-[#b65b45]'
      : item.tier === 'consequential'
        ? 'border-[#957040] before:bg-[#a07d45]'
        : 'border-[#5f594d] before:bg-[#70695c]';

  return (
    <div
      className={`relative overflow-hidden border bg-[#efe0c7] px-4 py-3 text-[#302114] shadow-[0_8px_16px_rgba(0,0,0,0.18)] before:absolute before:left-0 before:top-0 before:h-full before:w-1 ${accentClass}`}
    >
      <div className="pl-3">
        <div className="text-[11px] tracking-[0.28em] text-[#7f5e3a]">告示</div>
        <div className="mt-1 text-sm font-bold tracking-[0.08em]">{item.title}</div>
        <div className="mt-1 text-xs leading-5 text-[#5b4731]">{item.summary}</div>
      </div>
    </div>
  );
}

function NoticeTray() {
  return (
    <section className="relative h-full">
      <div className="absolute inset-0 translate-x-3 translate-y-3 bg-black/22" />
      <div className="relative flex h-full flex-col border border-[#8c6842] bg-[#efe0c7] px-5 py-5 text-[#2f2114] shadow-[0_18px_30px_rgba(0,0,0,0.28)]">
        <div className="border-b border-[#c4ab86] pb-3 text-center">
          <div className="text-[11px] tracking-[0.38em] text-[#896544]">告示托盘</div>
          <div className="mt-1 text-xl tracking-[0.14em]">今月诸事</div>
        </div>

        <div className="mt-4 space-y-5 overflow-y-auto pr-1">
          <div>
            <div className="mb-2 text-xs tracking-[0.3em] text-[#9b5440]">急报</div>
            <div className="space-y-3">
              {notices.filter((item) => item.tier === 'urgent').map((item) => <NoticeCard key={item.title} item={item} />)}
            </div>
          </div>

          <div>
            <div className="mb-2 text-xs tracking-[0.3em] text-[#8c6c44]">当月应理</div>
            <div className="space-y-3">
              {notices
                .filter((item) => item.tier === 'consequential')
                .map((item) => <NoticeCard key={item.title} item={item} />)}
            </div>
          </div>

          <div>
            <div className="mb-2 text-xs tracking-[0.3em] text-[#6e6659]">风闻与杂讯</div>
            <div className="space-y-3">
              {notices.filter((item) => item.tier === 'background').map((item) => <NoticeCard key={item.title} item={item} />)}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

function DeskNode({ node }: { node: CountyNode }) {
  return (
    <div className="absolute" style={{ left: node.left, top: node.top, transform: 'translate(-50%, -50%)' }}>
      <div
        className={`relative flex h-12 w-12 items-center justify-center rounded-full border text-[11px] tracking-[0.1em] shadow-lg ${
          node.active
            ? 'border-[#cf9b62] bg-[#4a2f1e] text-[#f3d7b4]'
            : 'border-[#7a5d3c] bg-[#2a1d14] text-[#d3b18c]'
        }`}
      >
        {node.name.slice(0, 2)}
      </div>
      <div className="mt-1 text-center text-[11px] text-[#cfb28a]">{node.name}</div>
      <div className="text-center text-[10px] text-[#9f8665]">{node.status}</div>
    </div>
  );
}

function DeskSandbox() {
  return (
    <section className="relative overflow-hidden border border-[#6e5336] bg-[linear-gradient(180deg,rgba(36,25,17,0.96),rgba(20,15,11,0.98))] px-5 py-5 shadow-[0_16px_30px_rgba(0,0,0,0.3)]">
      <div className="mb-4 flex items-center justify-between gap-4">
        <div>
          <div className="text-[11px] tracking-[0.35em] text-[#a68561]">案头县图</div>
          <div className="mt-1 text-lg tracking-[0.12em] text-[#ecdcc3]">县门躁、渡口缓、西市价浮</div>
        </div>
        <div className="text-xs text-[#a58a69]">是案头对象，不是全屏帝国沙盘</div>
      </div>

      <div className="relative h-[280px] overflow-hidden border border-[#5a4330] bg-[radial-gradient(circle_at_50%_42%,rgba(130,95,56,0.18),transparent_44%),linear-gradient(180deg,#2c1e14_0%,#1a130f_100%)]">
        <div className="absolute left-[14%] top-[59%] h-px w-[23%] rotate-[-18deg] bg-[#6e5337]" />
        <div className="absolute left-[35%] top-[33%] h-px w-[18%] rotate-[28deg] bg-[#6e5337]" />
        <div className="absolute left-[53%] top-[55%] h-px w-[18%] rotate-[-22deg] bg-[#6e5337]" />
        <div className="absolute left-[72%] top-[37%] h-px w-[8%] rotate-[36deg] bg-[#6e5337]" />

        {countyNodes.map((node) => (
          <DeskNode key={node.name} node={node} />
        ))}

        <div className="absolute bottom-4 left-4 w-[280px] border border-[#7e613f] bg-[#21170f]/92 px-4 py-3 shadow-lg">
          <div className="text-[11px] tracking-[0.3em] text-[#a78762]">当前热点</div>
          <div className="mt-2 text-sm text-[#ecd6ba]">县门榜示方出，人流与讼户并集。</div>
          <div className="mt-1 text-xs leading-5 text-[#9d8564]">
            若不先疏导与递报，街谈会比正信更早传回宗宅。
          </div>
        </div>
      </div>
    </section>
  );
}

export default function ZongzuHallShellPreview() {
  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-[#120d09] font-serif text-[#eadcc7]">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_20%,rgba(128,92,52,0.18),transparent_40%),linear-gradient(180deg,#24180f_0%,#17110d_45%,#100c09_100%)]" />
      <div className="absolute inset-0 opacity-20 [background-image:linear-gradient(rgba(255,255,255,0.03)_1px,transparent_1px),linear-gradient(90deg,rgba(255,255,255,0.03)_1px,transparent_1px)] [background-size:32px_32px]" />

      <div className="relative z-10 mx-auto flex min-h-screen max-w-[1560px] flex-col px-8 pb-8 pt-6">
        <header className="mb-5 flex items-center justify-between border-b border-[#6d5135]/70 pb-4">
          <div>
            <div className="text-[11px] tracking-[0.45em] text-[#a88763]">宗门记</div>
            <div className="mt-1 text-2xl tracking-[0.18em] text-[#f1e2cc]">汴州赵氏</div>
            <div className="mt-1 text-sm text-[#b69773]">熙宁二年 七月 · 堂上听事</div>
          </div>

          <div className="flex items-center gap-8 rounded-sm border border-[#6b4f34]/70 bg-[#22170f]/80 px-5 py-3 shadow-[0_10px_20px_rgba(0,0,0,0.25)]">
            <div>
              <div className="text-[10px] tracking-[0.28em] text-[#8c7356]">粮仓</div>
              <div className="mt-1 text-sm text-[#ead7b6]">八仓有余</div>
            </div>
            <div>
              <div className="text-[10px] tracking-[0.28em] text-[#8c7356]">族望</div>
              <div className="mt-1 text-sm text-[#ead7b6]">中上</div>
            </div>
            <div>
              <div className="text-[10px] tracking-[0.28em] text-[#8c7356]">人情</div>
              <div className="mt-1 text-sm text-[#ead7b6]">可调用三处</div>
            </div>
            <div>
              <div className="text-[10px] tracking-[0.28em] text-[#8c7356]">未阅</div>
              <div className="mt-1 text-sm text-[#ead7b6]">六件</div>
            </div>
          </div>
        </header>

        <div className="grid flex-1 grid-cols-[250px_minmax(0,1fr)_320px] gap-6">
          <aside className="flex flex-col gap-5">
            <FamilyLedger />
            <VisitorQueue />
          </aside>

          <main className="flex min-w-0 flex-col gap-5">
            <GreatHallLead />
            <DeskSandbox />
          </main>

          <aside>
            <NoticeTray />
          </aside>
        </div>
      </div>
    </div>
  );
}
