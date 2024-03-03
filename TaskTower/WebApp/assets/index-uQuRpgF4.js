import{R as $,r as G,j as e,c as k,u as r,a as ee,b as te,i as ae,t as q}from"./index-DYFrPAFf.js";const de=t=>{try{const n=localStorage.getItem(t);return typeof n=="string"?JSON.parse(n):void 0}catch{return}};function P(t,n){const[s,d]=$.useState();$.useEffect(()=>{const a=de(t);d(typeof a>"u"||a===null?typeof n=="function"?n():n:a)},[n,t]);const m=$.useCallback(a=>{d(j=>{let l=a;typeof a=="function"&&(l=a(j));try{localStorage.setItem(t,JSON.stringify(l))}catch{}return l})},[t]);return[s,m]}const ce=typeof window>"u";function W(t){return t.status==="success"&&t.isFetching?"blue":t.status==="pending"?"yellow":t.status==="error"?"red":t.status==="success"?"green":"gray"}function fe(t,n){const s=t.find(d=>d.routeId===n.id);return s?W(s):"gray"}function se(){const t=$.useRef(!1),n=$.useCallback(()=>t.current,[]);return $[ce?"useEffect":"useLayoutEffect"](()=>(t.current=!0,()=>{t.current=!1}),[]),n}const ue=t=>{const n=Object.getOwnPropertyNames(Object(t)),s=typeof t=="bigint"?`${t.toString()}n`:t;try{return JSON.stringify(s,n)}catch{return"unable to stringify"}};function Q(t){const n=se(),[s,d]=$.useState(t),m=$.useCallback(a=>{xe(()=>{n()&&d(a)})},[n]);return[s,m]}function xe(t){Promise.resolve().then(t).catch(n=>setTimeout(()=>{throw n}))}function pe(t,n=[s=>s]){return t.map((s,d)=>[s,d]).sort(([s,d],[m,a])=>{for(const j of n){const l=j(s),x=j(m);if(typeof l>"u"){if(typeof x>"u")continue;return 1}if(l!==x)return l>x?1:-1}return d-a}).map(([s])=>s)}const v={colors:{inherit:"inherit",current:"currentColor",transparent:"transparent",black:"#000000",white:"#ffffff",neutral:{50:"#f9fafb",100:"#f2f4f7",200:"#eaecf0",300:"#d0d5dd",400:"#98a2b3",500:"#667085",600:"#475467",700:"#344054",800:"#1d2939",900:"#101828"},darkGray:{50:"#525c7a",100:"#49536e",200:"#414962",300:"#394056",400:"#313749",500:"#292e3d",600:"#212530",700:"#191c24",800:"#111318",900:"#0b0d10"},gray:{50:"#f9fafb",100:"#f2f4f7",200:"#eaecf0",300:"#d0d5dd",400:"#98a2b3",500:"#667085",600:"#475467",700:"#344054",800:"#1d2939",900:"#101828"},blue:{25:"#F5FAFF",50:"#EFF8FF",100:"#D1E9FF",200:"#B2DDFF",300:"#84CAFF",400:"#53B1FD",500:"#2E90FA",600:"#1570EF",700:"#175CD3",800:"#1849A9",900:"#194185"},green:{25:"#F6FEF9",50:"#ECFDF3",100:"#D1FADF",200:"#A6F4C5",300:"#6CE9A6",400:"#32D583",500:"#12B76A",600:"#039855",700:"#027A48",800:"#05603A",900:"#054F31"},red:{50:"#fef2f2",100:"#fee2e2",200:"#fecaca",300:"#fca5a5",400:"#f87171",500:"#ef4444",600:"#dc2626",700:"#b91c1c",800:"#991b1b",900:"#7f1d1d",950:"#450a0a"},yellow:{25:"#FFFCF5",50:"#FFFAEB",100:"#FEF0C7",200:"#FEDF89",300:"#FEC84B",400:"#FDB022",500:"#F79009",600:"#DC6803",700:"#B54708",800:"#93370D",900:"#7A2E0E"},purple:{25:"#FAFAFF",50:"#F4F3FF",100:"#EBE9FE",200:"#D9D6FE",300:"#BDB4FE",400:"#9B8AFB",500:"#7A5AF8",600:"#6938EF",700:"#5925DC",800:"#4A1FB8",900:"#3E1C96"},teal:{25:"#F6FEFC",50:"#F0FDF9",100:"#CCFBEF",200:"#99F6E0",300:"#5FE9D0",400:"#2ED3B7",500:"#15B79E",600:"#0E9384",700:"#107569",800:"#125D56",900:"#134E48"},pink:{25:"#fdf2f8",50:"#fce7f3",100:"#fbcfe8",200:"#f9a8d4",300:"#f472b6",400:"#ec4899",500:"#db2777",600:"#be185d",700:"#9d174d",800:"#831843",900:"#500724"},cyan:{25:"#ecfeff",50:"#cffafe",100:"#a5f3fc",200:"#67e8f9",300:"#22d3ee",400:"#06b6d4",500:"#0891b2",600:"#0e7490",700:"#155e75",800:"#164e63",900:"#083344"}},alpha:{100:"ff",90:"e5",80:"cc",70:"b3",60:"99",50:"80",40:"66",30:"4d",20:"33",10:"1a",0:"00"},font:{size:{"2xs":"calc(var(--tsrd-font-size) * 0.625)",xs:"calc(var(--tsrd-font-size) * 0.75)",sm:"calc(var(--tsrd-font-size) * 0.875)",md:"var(--tsrd-font-size)",lg:"calc(var(--tsrd-font-size) * 1.125)",xl:"calc(var(--tsrd-font-size) * 1.25)","2xl":"calc(var(--tsrd-font-size) * 1.5)","3xl":"calc(var(--tsrd-font-size) * 1.875)","4xl":"calc(var(--tsrd-font-size) * 2.25)","5xl":"calc(var(--tsrd-font-size) * 3)","6xl":"calc(var(--tsrd-font-size) * 3.75)","7xl":"calc(var(--tsrd-font-size) * 4.5)","8xl":"calc(var(--tsrd-font-size) * 6)","9xl":"calc(var(--tsrd-font-size) * 8)"},lineHeight:{"3xs":"calc(var(--tsrd-font-size) * 0.75)","2xs":"calc(var(--tsrd-font-size) * 0.875)",xs:"calc(var(--tsrd-font-size) * 1)",sm:"calc(var(--tsrd-font-size) * 1.25)",md:"calc(var(--tsrd-font-size) * 1.5)",lg:"calc(var(--tsrd-font-size) * 1.75)",xl:"calc(var(--tsrd-font-size) * 2)","2xl":"calc(var(--tsrd-font-size) * 2.25)","3xl":"calc(var(--tsrd-font-size) * 2.5)","4xl":"calc(var(--tsrd-font-size) * 2.75)","5xl":"calc(var(--tsrd-font-size) * 3)","6xl":"calc(var(--tsrd-font-size) * 3.25)","7xl":"calc(var(--tsrd-font-size) * 3.5)","8xl":"calc(var(--tsrd-font-size) * 3.75)","9xl":"calc(var(--tsrd-font-size) * 4)"},weight:{thin:"100",extralight:"200",light:"300",normal:"400",medium:"500",semibold:"600",bold:"700",extrabold:"800",black:"900"},fontFamily:{sans:"ui-sans-serif, Inter, system-ui, sans-serif, sans-serif",mono:"ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace"}},breakpoints:{xs:"320px",sm:"640px",md:"768px",lg:"1024px",xl:"1280px","2xl":"1536px"},border:{radius:{none:"0px",xs:"calc(var(--tsrd-font-size) * 0.125)",sm:"calc(var(--tsrd-font-size) * 0.25)",md:"calc(var(--tsrd-font-size) * 0.375)",lg:"calc(var(--tsrd-font-size) * 0.5)",xl:"calc(var(--tsrd-font-size) * 0.75)","2xl":"calc(var(--tsrd-font-size) * 1)","3xl":"calc(var(--tsrd-font-size) * 1.5)",full:"9999px"}},size:{0:"0px",.25:"calc(var(--tsrd-font-size) * 0.0625)",.5:"calc(var(--tsrd-font-size) * 0.125)",1:"calc(var(--tsrd-font-size) * 0.25)",1.5:"calc(var(--tsrd-font-size) * 0.375)",2:"calc(var(--tsrd-font-size) * 0.5)",2.5:"calc(var(--tsrd-font-size) * 0.625)",3:"calc(var(--tsrd-font-size) * 0.75)",3.5:"calc(var(--tsrd-font-size) * 0.875)",4:"calc(var(--tsrd-font-size) * 1)",4.5:"calc(var(--tsrd-font-size) * 1.125)",5:"calc(var(--tsrd-font-size) * 1.25)",5.5:"calc(var(--tsrd-font-size) * 1.375)",6:"calc(var(--tsrd-font-size) * 1.5)",6.5:"calc(var(--tsrd-font-size) * 1.625)",7:"calc(var(--tsrd-font-size) * 1.75)",8:"calc(var(--tsrd-font-size) * 2)",9:"calc(var(--tsrd-font-size) * 2.25)",10:"calc(var(--tsrd-font-size) * 2.5)",11:"calc(var(--tsrd-font-size) * 2.75)",12:"calc(var(--tsrd-font-size) * 3)",14:"calc(var(--tsrd-font-size) * 3.5)",16:"calc(var(--tsrd-font-size) * 4)",20:"calc(var(--tsrd-font-size) * 5)",24:"calc(var(--tsrd-font-size) * 6)",28:"calc(var(--tsrd-font-size) * 7)",32:"calc(var(--tsrd-font-size) * 8)",36:"calc(var(--tsrd-font-size) * 9)",40:"calc(var(--tsrd-font-size) * 10)",44:"calc(var(--tsrd-font-size) * 11)",48:"calc(var(--tsrd-font-size) * 12)",52:"calc(var(--tsrd-font-size) * 13)",56:"calc(var(--tsrd-font-size) * 14)",60:"calc(var(--tsrd-font-size) * 15)",64:"calc(var(--tsrd-font-size) * 16)",72:"calc(var(--tsrd-font-size) * 18)",80:"calc(var(--tsrd-font-size) * 20)",96:"calc(var(--tsrd-font-size) * 24)"},shadow:{xs:(t="rgb(0 0 0 / 0.1)")=>"0 1px 2px 0 rgb(0 0 0 / 0.05)",sm:(t="rgb(0 0 0 / 0.1)")=>`0 1px 3px 0 ${t}, 0 1px 2px -1px ${t}`,md:(t="rgb(0 0 0 / 0.1)")=>`0 4px 6px -1px ${t}, 0 2px 4px -2px ${t}`,lg:(t="rgb(0 0 0 / 0.1)")=>`0 10px 15px -3px ${t}, 0 4px 6px -4px ${t}`,xl:(t="rgb(0 0 0 / 0.1)")=>`0 20px 25px -5px ${t}, 0 8px 10px -6px ${t}`,"2xl":(t="rgb(0 0 0 / 0.25)")=>`0 25px 50px -12px ${t}`,inner:(t="rgb(0 0 0 / 0.05)")=>`inset 0 2px 4px 0 ${t}`,none:()=>"none"},zIndices:{hide:-1,auto:"auto",base:0,docked:10,dropdown:1e3,sticky:1100,banner:1200,overlay:1300,modal:1400,popover:1500,skipLink:1600,toast:1700,tooltip:1800}},X=({expanded:t,style:n={}})=>e.jsx("span",{className:z().expander,children:e.jsx("svg",{xmlns:"http://www.w3.org/2000/svg",width:"12",height:"12",fill:"none",viewBox:"0 0 24 24",className:k(z().expanderIcon(t)),children:e.jsx("path",{stroke:"currentColor",strokeLinecap:"round",strokeLinejoin:"round",strokeWidth:"2",d:"M9 18l6-6-6-6"})})});function he(t,n){if(n<1)return[];let s=0;const d=[];for(;s<t.length;)d.push(t.slice(s,s+n)),s=s+n;return d}const me=({handleEntry:t,label:n,value:s,subEntries:d=[],subEntryPages:m=[],type:a,expanded:j=!1,toggleExpanded:l,pageSize:x,renderer:i})=>{const[f,u]=G.useState([]),[g,h]=G.useState(void 0),b=()=>{h(s())};return e.jsx("div",{className:z().entry,children:m.length?e.jsxs(e.Fragment,{children:[e.jsxs("button",{className:z().expandButton,onClick:()=>l(),children:[e.jsx(X,{expanded:j}),n,e.jsxs("span",{className:z().info,children:[String(a).toLowerCase()==="iterable"?"(Iterable) ":"",d.length," ",d.length>1?"items":"item"]})]}),j?m.length===1?e.jsx("div",{className:z().subEntries,children:d.map((C,p)=>t(C))}):e.jsx("div",{className:z().subEntries,children:m.map((C,p)=>e.jsx("div",{children:e.jsxs("div",{className:z().entry,children:[e.jsxs("button",{className:k(z().labelButton,"labelButton"),onClick:()=>u(F=>F.includes(p)?F.filter(R=>R!==p):[...F,p]),children:[e.jsx(X,{expanded:f.includes(p)})," ","[",p*x," ..."," ",p*x+x-1,"]"]}),f.includes(p)?e.jsx("div",{className:z().subEntries,children:C.map(F=>t(F))}):null]})},p))}):null]}):a==="function"?e.jsx(e.Fragment,{children:e.jsx(D,{renderer:i,label:e.jsxs("button",{onClick:b,className:z().refreshValueBtn,children:[e.jsx("span",{children:n})," 🔄"," "]}),value:g,defaultExpanded:{}})}):e.jsxs(e.Fragment,{children:[e.jsxs("span",{children:[n,":"]})," ",e.jsx("span",{className:z().value,children:ue(s)})]})})};function ge(t){return Symbol.iterator in t}function D({value:t,defaultExpanded:n,renderer:s=me,pageSize:d=100,filterSubEntries:m,...a}){const[j,l]=G.useState(!!n),x=G.useCallback(()=>l(h=>!h),[]);let i=typeof t,f=[];const u=h=>{const b=n===!0?{[h.label]:!0}:n==null?void 0:n[h.label];return{...h,defaultExpanded:b}};Array.isArray(t)?(i="array",f=t.map((h,b)=>u({label:b.toString(),value:h}))):t!==null&&typeof t=="object"&&ge(t)&&typeof t[Symbol.iterator]=="function"?(i="Iterable",f=Array.from(t,(h,b)=>u({label:b.toString(),value:h}))):typeof t=="object"&&t!==null&&(i="object",f=Object.entries(t).map(([h,b])=>u({label:h,value:b}))),f=m?m(f):f;const g=he(f,d);return s({handleEntry:h=>e.jsx(D,{value:t,renderer:s,filterSubEntries:m,...a,...h},h.label),type:i,subEntries:f,subEntryPages:g,value:t,expanded:j,toggleExpanded:x,pageSize:d,...a})}const ve=()=>{const{colors:t,font:n,size:s,alpha:d,shadow:m,border:a}=v,{fontFamily:j,lineHeight:l,size:x}=n;return{entry:r`
      font-family: ${j.mono};
      font-size: ${x.xs};
      line-height: ${l.sm};
      outline: none;
      word-break: break-word;
    `,labelButton:r`
      cursor: pointer;
      color: inherit;
      font: inherit;
      outline: inherit;
      background: transparent;
      border: none;
      padding: 0;
    `,expander:r`
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: ${s[3]};
      height: ${s[3]};
      padding-left: 3px;
      box-sizing: content-box;
    `,expanderIcon:i=>i?r`
          transform: rotate(90deg);
          transition: transform 0.1s ease;
        `:r`
        transform: rotate(0deg);
        transition: transform 0.1s ease;
      `,expandButton:r`
      display: flex;
      gap: ${s[1]};
      align-items: center;
      cursor: pointer;
      color: inherit;
      font: inherit;
      outline: inherit;
      background: transparent;
      border: none;
      padding: 0;
    `,value:r`
      color: ${t.purple[400]};
    `,subEntries:r`
      margin-left: ${s[2]};
      padding-left: ${s[2]};
      border-left: 2px solid ${t.darkGray[400]};
    `,info:r`
      color: ${t.gray[500]};
      font-size: ${x["2xs"]};
      padding-left: ${s[1]};
    `,refreshValueBtn:r`
      appearance: none;
      border: 0;
      cursor: pointer;
      background: transparent;
      color: inherit;
      padding: 0;
      font-family: ${j.mono};
      font-size: ${x.xs};
    `}};let T=null;function z(){return T||(T=ve(),T)}function Z(){const t=$.useId();return e.jsx("svg",{xmlns:"http://www.w3.org/2000/svg",enableBackground:"new 0 0 634 633",viewBox:"0 0 634 633",children:e.jsxs("g",{transform:"translate(1)",children:[e.jsxs("linearGradient",{id:`a-${t}`,x1:"-641.486",x2:"-641.486",y1:"856.648",y2:"855.931",gradientTransform:"matrix(633 0 0 -633 406377 542258)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#6bdaff"}),e.jsx("stop",{offset:"0.319",stopColor:"#f9ffb5"}),e.jsx("stop",{offset:"0.706",stopColor:"#ffa770"}),e.jsx("stop",{offset:"1",stopColor:"#ff7373"})]}),e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:`url(#a-${t})`,fillRule:"evenodd",clipRule:"evenodd"}),e.jsx("defs",{children:e.jsx("filter",{id:`b-${t}`,width:"454",height:"396.9",x:"-137.5",y:"412",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`c-${t}`,width:"454",height:"396.9",x:"-137.5",y:"412",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#b-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsx("ellipse",{cx:"89.5",cy:"610.5",fill:"#015064",fillRule:"evenodd",stroke:"#00CFE2",strokeWidth:"25",clipRule:"evenodd",mask:`url(#c-${t})`,rx:"214.5",ry:"186"}),e.jsx("defs",{children:e.jsx("filter",{id:`d-${t}`,width:"454",height:"396.9",x:"316.5",y:"412",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`e-${t}`,width:"454",height:"396.9",x:"316.5",y:"412",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#d-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsx("ellipse",{cx:"543.5",cy:"610.5",fill:"#015064",fillRule:"evenodd",stroke:"#00CFE2",strokeWidth:"25",clipRule:"evenodd",mask:`url(#e-${t})`,rx:"214.5",ry:"186"}),e.jsx("defs",{children:e.jsx("filter",{id:`f-${t}`,width:"454",height:"396.9",x:"-137.5",y:"450",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`g-${t}`,width:"454",height:"396.9",x:"-137.5",y:"450",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#f-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsx("ellipse",{cx:"89.5",cy:"648.5",fill:"#015064",fillRule:"evenodd",stroke:"#00A8B8",strokeWidth:"25",clipRule:"evenodd",mask:`url(#g-${t})`,rx:"214.5",ry:"186"}),e.jsx("defs",{children:e.jsx("filter",{id:`h-${t}`,width:"454",height:"396.9",x:"316.5",y:"450",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`i-${t}`,width:"454",height:"396.9",x:"316.5",y:"450",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#h-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsx("ellipse",{cx:"543.5",cy:"648.5",fill:"#015064",fillRule:"evenodd",stroke:"#00A8B8",strokeWidth:"25",clipRule:"evenodd",mask:`url(#i-${t})`,rx:"214.5",ry:"186"}),e.jsx("defs",{children:e.jsx("filter",{id:`j-${t}`,width:"454",height:"396.9",x:"-137.5",y:"486",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`k-${t}`,width:"454",height:"396.9",x:"-137.5",y:"486",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#j-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsx("ellipse",{cx:"89.5",cy:"684.5",fill:"#015064",fillRule:"evenodd",stroke:"#007782",strokeWidth:"25",clipRule:"evenodd",mask:`url(#k-${t})`,rx:"214.5",ry:"186"}),e.jsx("defs",{children:e.jsx("filter",{id:`l-${t}`,width:"454",height:"396.9",x:"316.5",y:"486",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`m-${t}`,width:"454",height:"396.9",x:"316.5",y:"486",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#l-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsx("ellipse",{cx:"543.5",cy:"684.5",fill:"#015064",fillRule:"evenodd",stroke:"#007782",strokeWidth:"25",clipRule:"evenodd",mask:`url(#m-${t})`,rx:"214.5",ry:"186"}),e.jsx("defs",{children:e.jsx("filter",{id:`n-${t}`,width:"176.9",height:"129.3",x:"272.2",y:"308",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`o-${t}`,width:"176.9",height:"129.3",x:"272.2",y:"308",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#n-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsxs("g",{mask:`url(#o-${t})`,children:[e.jsx("path",{fill:"none",stroke:"#000",strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"11",d:"M436 403.2l-5 28.6m-140-90.3l-10.9 62m52.8-19.4l-4.3 27.1"}),e.jsxs("linearGradient",{id:`p-${t}`,x1:"-645.656",x2:"-646.499",y1:"854.878",y2:"854.788",gradientTransform:"matrix(-184.159 -32.4722 11.4608 -64.9973 -128419.844 34938.836)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ee2700"}),e.jsx("stop",{offset:"1",stopColor:"#ff008e"})]}),e.jsx("path",{fill:`url(#p-${t})`,fillRule:"evenodd",d:"M344.1 363l97.7 17.2c5.8 2.1 8.2 6.2 7.1 12.1-1 5.9-4.7 9.2-11 9.9l-106-18.7-57.5-59.2c-3.2-4.8-2.9-9.1.8-12.8 3.7-3.7 8.3-4.4 13.7-2.1l55.2 53.6z",clipRule:"evenodd"}),e.jsx("path",{fill:"#D8D8D8",fillRule:"evenodd",stroke:"#FFF",strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"7",d:"M428.3 384.5l.9-6.5m-33.9 1.5l.9-6.5m-34 .5l.9-6.1m-38.9-16.1l4.2-3.9m-25.2-16.1l4.2-3.9",clipRule:"evenodd"})]}),e.jsx("defs",{children:e.jsx("filter",{id:`q-${t}`,width:"280.6",height:"317.4",x:"73.2",y:"113.9",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`r-${t}`,width:"280.6",height:"317.4",x:"73.2",y:"113.9",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#q-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsxs("g",{mask:`url(#r-${t})`,children:[e.jsxs("linearGradient",{id:`s-${t}`,x1:"-646.8",x2:"-646.8",y1:"854.844",y2:"853.844",gradientTransform:"matrix(-100.1751 48.8587 -97.9753 -200.879 19124.773 203538.61)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#a17500"}),e.jsx("stop",{offset:"1",stopColor:"#5d2100"})]}),e.jsx("path",{fill:`url(#s-${t})`,fillRule:"evenodd",d:"M192.3 203c8.1 37.3 14 73.6 17.8 109.1 3.8 35.4 2.8 75.2-2.9 119.2l61.2-16.7c-15.6-59-25.2-97.9-28.6-116.6-3.4-18.7-10.8-51.8-22.2-99.6l-25.3 4.6",clipRule:"evenodd"}),e.jsxs("linearGradient",{id:`t-${t}`,x1:"-635.467",x2:"-635.467",y1:"852.115",y2:"851.115",gradientTransform:"matrix(92.6873 4.8575 2.0257 -38.6535 57323.695 36176.047)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#2f8a00"}),e.jsx("stop",{offset:"1",stopColor:"#90ff57"})]}),e.jsx("path",{fill:`url(#t-${t})`,fillRule:"evenodd",stroke:"#2F8A00",strokeWidth:"13",d:"M195 183.9s-12.6-22.1-36.5-29.9c-15.9-5.2-34.4-1.5-55.5 11.1 15.9 14.3 29.5 22.6 40.7 24.9 16.8 3.6 51.3-6.1 51.3-6.1z",clipRule:"evenodd"}),e.jsxs("linearGradient",{id:`u-${t}`,x1:"-636.573",x2:"-636.573",y1:"855.444",y2:"854.444",gradientTransform:"matrix(109.9945 5.7646 6.3597 -121.3507 64719.133 107659.336)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#2f8a00"}),e.jsx("stop",{offset:"1",stopColor:"#90ff57"})]}),e.jsx("path",{fill:`url(#u-${t})`,fillRule:"evenodd",stroke:"#2F8A00",strokeWidth:"13",d:"M194.9 184.5s-47.5-8.5-83.2 15.7c-23.8 16.2-34.3 49.3-31.6 99.3 30.3-27.8 52.1-48.5 65.2-61.9 19.8-20 49.6-53.1 49.6-53.1z",clipRule:"evenodd"}),e.jsxs("linearGradient",{id:`v-${t}`,x1:"-632.145",x2:"-632.145",y1:"854.174",y2:"853.174",gradientTransform:"matrix(62.9558 3.2994 3.5021 -66.8246 37035.367 59284.227)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#2f8a00"}),e.jsx("stop",{offset:"1",stopColor:"#90ff57"})]}),e.jsx("path",{fill:`url(#v-${t})`,fillRule:"evenodd",stroke:"#2F8A00",strokeWidth:"13",d:"M195 183.9c-.8-21.9 6-38 20.6-48.2 14.6-10.2 29.8-15.3 45.5-15.3-6.1 21.4-14.5 35.8-25.2 43.4-10.7 7.5-24.4 14.2-40.9 20.1z",clipRule:"evenodd"}),e.jsxs("linearGradient",{id:`w-${t}`,x1:"-638.224",x2:"-638.224",y1:"853.801",y2:"852.801",gradientTransform:"matrix(152.4666 7.9904 3.0934 -59.0251 94939.86 55646.855)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#2f8a00"}),e.jsx("stop",{offset:"1",stopColor:"#90ff57"})]}),e.jsx("path",{fill:`url(#w-${t})`,fillRule:"evenodd",stroke:"#2F8A00",strokeWidth:"13",d:"M194.9 184.5c31.9-30 64.1-39.7 96.7-29 32.6 10.7 50.8 30.4 54.6 59.1-35.2-5.5-60.4-9.6-75.8-12.1-15.3-2.6-40.5-8.6-75.5-18z",clipRule:"evenodd"}),e.jsxs("linearGradient",{id:`x-${t}`,x1:"-637.723",x2:"-637.723",y1:"855.103",y2:"854.103",gradientTransform:"matrix(136.467 7.1519 5.2165 -99.5377 82830.875 89859.578)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#2f8a00"}),e.jsx("stop",{offset:"1",stopColor:"#90ff57"})]}),e.jsx("path",{fill:`url(#x-${t})`,fillRule:"evenodd",stroke:"#2F8A00",strokeWidth:"13",d:"M194.9 184.5c35.8-7.6 65.6-.2 89.2 22 23.6 22.2 37.7 49 42.3 80.3-39.8-9.7-68.3-23.8-85.5-42.4-17.2-18.5-32.5-38.5-46-59.9z",clipRule:"evenodd"}),e.jsxs("linearGradient",{id:`y-${t}`,x1:"-631.79",x2:"-631.79",y1:"855.872",y2:"854.872",gradientTransform:"matrix(60.8683 3.19 8.7771 -167.4773 31110.818 145537.61)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#2f8a00"}),e.jsx("stop",{offset:"1",stopColor:"#90ff57"})]}),e.jsx("path",{fill:`url(#y-${t})`,fillRule:"evenodd",stroke:"#2F8A00",strokeWidth:"13",d:"M194.9 184.5c-33.6 13.8-53.6 35.7-60.1 65.6-6.5 29.9-3.6 63.1 8.7 99.6 27.4-40.3 43.2-69.6 47.4-88 4.2-18.3 5.5-44.1 4-77.2z",clipRule:"evenodd"}),e.jsx("path",{fill:"none",stroke:"#2F8A00",strokeLinecap:"round",strokeWidth:"8",d:"M196.5 182.3c-14.8 21.6-25.1 41.4-30.8 59.4-5.7 18-9.4 33-11.1 45.1"}),e.jsx("path",{fill:"none",stroke:"#2F8A00",strokeLinecap:"round",strokeWidth:"8",d:"M194.8 185.7c-24.4 1.7-43.8 9-58.1 21.8-14.3 12.8-24.7 25.4-31.3 37.8m99.1-68.9c29.7-6.7 52-8.4 67-5 15 3.4 26.9 8.7 35.8 15.9m-110.8-5.9c20.3 9.9 38.2 20.5 53.9 31.9 15.7 11.4 27.4 22.1 35.1 32"})]}),e.jsx("defs",{children:e.jsx("filter",{id:`z-${t}`,width:"532",height:"633",x:"50.5",y:"399",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`A-${t}`,width:"532",height:"633",x:"50.5",y:"399",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#z-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsxs("linearGradient",{id:`B-${t}`,x1:"-641.104",x2:"-641.278",y1:"856.577",y2:"856.183",gradientTransform:"matrix(532 0 0 -633 341484.5 542657)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#fff400"}),e.jsx("stop",{offset:"1",stopColor:"#3c8700"})]}),e.jsx("ellipse",{cx:"316.5",cy:"715.5",fill:`url(#B-${t})`,fillRule:"evenodd",clipRule:"evenodd",mask:`url(#A-${t})`,rx:"266",ry:"316.5"}),e.jsx("defs",{children:e.jsx("filter",{id:`C-${t}`,width:"288",height:"283",x:"391",y:"-24",filterUnits:"userSpaceOnUse",children:e.jsx("feColorMatrix",{values:"1 0 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 1 0"})})}),e.jsx("mask",{id:`D-${t}`,width:"288",height:"283",x:"391",y:"-24",maskUnits:"userSpaceOnUse",children:e.jsx("g",{filter:`url(#C-${t})`,children:e.jsx("circle",{cx:"316.5",cy:"316.5",r:"316.5",fill:"#FFF",fillRule:"evenodd",clipRule:"evenodd"})})}),e.jsx("g",{mask:`url(#D-${t})`,children:e.jsxs("g",{transform:"translate(397 -24)",children:[e.jsxs("linearGradient",{id:`E-${t}`,x1:"-1036.672",x2:"-1036.672",y1:"880.018",y2:"879.018",gradientTransform:"matrix(227 0 0 -227 235493 199764)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ffdf00"}),e.jsx("stop",{offset:"1",stopColor:"#ff9d00"})]}),e.jsx("circle",{cx:"168.5",cy:"113.5",r:"113.5",fill:`url(#E-${t})`,fillRule:"evenodd",clipRule:"evenodd"}),e.jsxs("linearGradient",{id:`F-${t}`,x1:"-1017.329",x2:"-1018.602",y1:"658.003",y2:"657.998",gradientTransform:"matrix(30 0 0 -1 30558 771)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ffa400"}),e.jsx("stop",{offset:"1",stopColor:"#ff5e00"})]}),e.jsx("path",{fill:"none",stroke:`url(#F-${t})`,strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"12",d:"M30 113H0"}),e.jsxs("linearGradient",{id:`G-${t}`,x1:"-1014.501",x2:"-1015.774",y1:"839.985",y2:"839.935",gradientTransform:"matrix(26.5 0 0 -5.5 26925 4696.5)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ffa400"}),e.jsx("stop",{offset:"1",stopColor:"#ff5e00"})]}),e.jsx("path",{fill:"none",stroke:`url(#G-${t})`,strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"12",d:"M33.5 79.5L7 74"}),e.jsxs("linearGradient",{id:`H-${t}`,x1:"-1016.59",x2:"-1017.862",y1:"852.671",y2:"852.595",gradientTransform:"matrix(29 0 0 -8 29523 6971)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ffa400"}),e.jsx("stop",{offset:"1",stopColor:"#ff5e00"})]}),e.jsx("path",{fill:"none",stroke:`url(#H-${t})`,strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"12",d:"M34 146l-29 8"}),e.jsxs("linearGradient",{id:`I-${t}`,x1:"-1011.984",x2:"-1013.257",y1:"863.523",y2:"863.229",gradientTransform:"matrix(24 0 0 -13 24339 11407)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ffa400"}),e.jsx("stop",{offset:"1",stopColor:"#ff5e00"})]}),e.jsx("path",{fill:"none",stroke:`url(#I-${t})`,strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"12",d:"M45 177l-24 13"}),e.jsxs("linearGradient",{id:`J-${t}`,x1:"-1006.673",x2:"-1007.946",y1:"869.279",y2:"868.376",gradientTransform:"matrix(20 0 0 -19 20205 16720)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ffa400"}),e.jsx("stop",{offset:"1",stopColor:"#ff5e00"})]}),e.jsx("path",{fill:"none",stroke:`url(#J-${t})`,strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"12",d:"M67 204l-20 19"}),e.jsxs("linearGradient",{id:`K-${t}`,x1:"-992.85",x2:"-993.317",y1:"871.258",y2:"870.258",gradientTransform:"matrix(13.8339 0 0 -22.8467 13825.796 20131.938)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ffa400"}),e.jsx("stop",{offset:"1",stopColor:"#ff5e00"})]}),e.jsx("path",{fill:"none",stroke:`url(#K-${t})`,strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"12",d:"M94.4 227l-13.8 22.8"}),e.jsxs("linearGradient",{id:`L-${t}`,x1:"-953.835",x2:"-953.965",y1:"871.9",y2:"870.9",gradientTransform:"matrix(7.5 0 0 -24.5 7278 21605)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ffa400"}),e.jsx("stop",{offset:"1",stopColor:"#ff5e00"})]}),e.jsx("path",{fill:"none",stroke:`url(#L-${t})`,strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"12",d:"M127.5 243.5L120 268"}),e.jsxs("linearGradient",{id:`M-${t}`,x1:"244.504",x2:"244.496",y1:"871.898",y2:"870.898",gradientTransform:"matrix(.5 0 0 -24.5 45.5 21614)",gradientUnits:"userSpaceOnUse",children:[e.jsx("stop",{offset:"0",stopColor:"#ffa400"}),e.jsx("stop",{offset:"1",stopColor:"#ff5e00"})]}),e.jsx("path",{fill:"none",stroke:`url(#M-${t})`,strokeLinecap:"round",strokeLinejoin:"bevel",strokeWidth:"12",d:"M167.5 252.5l.5 24.5"})]})})]})})}function je(t){const{className:n,...s}=t;return e.jsxs("button",{...s,className:k(o().logo,n),children:[e.jsx("div",{className:o().tanstackLogo,children:"TANSTACK"}),e.jsx("div",{className:o().routerLogo,children:"React Router v1"})]})}const re=$.createContext(void 0),$e=()=>{const t=$.useContext(re);if(!t)throw new Error("useDevtoolsOnClose must be used within a TanStackRouterDevtools component");return t};function Ce({initialIsOpen:t,panelProps:n={},closeButtonProps:s={},toggleButtonProps:d={},position:m="bottom-left",containerElement:a="footer",router:j}){const[l,x]=$.useState(null),i=$.useRef(null),[f,u]=P("tanstackRouterDevtoolsOpen",t),[g,h]=P("tanstackRouterDevtoolsHeight",null),[b,C]=Q(!1),[p,F]=Q(!1),R=se(),O=(w,U)=>{if(U.button!==0)return;F(!0);const B={originalHeight:(w==null?void 0:w.getBoundingClientRect().height)??0,pageY:U.pageY},M=ie=>{const le=B.pageY-ie.pageY,K=(B==null?void 0:B.originalHeight)+le;h(K),K<70?u(!1):u(!0)},I=()=>{F(!1),document.removeEventListener("mousemove",M),document.removeEventListener("mouseUp",I)};document.addEventListener("mousemove",M),document.addEventListener("mouseup",I)},S=f??!1;$.useEffect(()=>{C(f??!1)},[f,b,C]),$.useEffect(()=>{var w;if(b){const U=(w=l==null?void 0:l.parentElement)==null?void 0:w.style.paddingBottom,B=()=>{var M;const I=(M=i.current)==null?void 0:M.getBoundingClientRect().height;l!=null&&l.parentElement&&(l.parentElement.style.paddingBottom=`${I}px`)};if(B(),typeof window<"u")return window.addEventListener("resize",B),()=>{window.removeEventListener("resize",B),l!=null&&l.parentElement&&typeof U=="string"&&(l.parentElement.style.paddingBottom=U)}}},[b]),$.useEffect(()=>{if(l){const w=l,U=getComputedStyle(w).fontSize;w.style.setProperty("--tsrd-font-size",U)}},[l]);const{style:E={},...y}=n,{style:V={},onClick:L,...c}=s,{style:N={},onClick:Y,...oe}=d;if(!R())return null;const J=g??500;return e.jsxs(a,{ref:x,className:"TanStackRouterDevtools",children:[e.jsx(re.Provider,{value:{onCloseClick:L??(()=>{})},children:e.jsx(ye,{ref:i,...y,router:j,className:k(o().devtoolsPanelContainer,o().devtoolsPanelContainerVisibility(!!f),o().devtoolsPanelContainerResizing(p),o().devtoolsPanelContainerAnimation(b,J+16)),style:{height:J,...E},isOpen:b,setIsOpen:u,handleDragStart:w=>O(i.current,w)})}),e.jsxs("button",{type:"button",...oe,"aria-label":"Open TanStack Router Devtools",onClick:w=>{u(!0),Y&&Y(w)},className:k(o().mainCloseBtn,o().mainCloseBtnPosition(m),o().mainCloseBtnAnimation(!S)),children:[e.jsxs("div",{className:o().mainCloseBtnIconContainer,children:[e.jsx("div",{className:o().mainCloseBtnIconOuter,children:e.jsx(Z,{})}),e.jsx("div",{className:o().mainCloseBtnIconInner,children:e.jsx(Z,{})})]}),e.jsx("div",{className:o().mainCloseBtnDivider,children:"-"}),e.jsx("div",{className:o().routerLogoCloseButton,children:"React Router"})]})]})}function ne({route:t,isRoot:n,activeId:s,setActiveId:d}){var m;const a=te(),j=a.status==="pending"?a.pendingMatches??[]:a.matches,l=a.matches.find(i=>i.routeId===t.id),x=$.useMemo(()=>{try{if(l!=null&&l.params){const i=l.params,f=t.path||q(t.id);if(f.startsWith("$")){const u=f.slice(1);if(i[u])return`(${i[u]})`}}return""}catch{return""}},[l,t]);return e.jsxs("div",{children:[e.jsxs("div",{role:"button","aria-label":`Open match details for ${t.id}`,onClick:()=>{l&&d(s===t.id?"":t.id)},className:k(o().routesRowContainer(t.id===s,!!l)),children:[e.jsx("div",{className:k(o().matchIndicator(fe(j,t)))}),e.jsxs("div",{className:k(o().routesRow(!!l)),children:[e.jsxs("div",{children:[e.jsxs("code",{className:o().code,children:[n?"__root__":t.path||q(t.id)," "]}),e.jsx("code",{className:o().routeParamInfo,children:x})]}),e.jsx(_,{match:l})]})]}),(m=t.children)!=null&&m.length?e.jsx("div",{className:o().nestedRouteRow(!!n),children:[...t.children].sort((i,f)=>i.rank-f.rank).map(i=>e.jsx(ne,{route:i,activeId:s,setActiveId:d},i.id))}):null]})}const ye=$.forwardRef(function(n,s){var d,m,a;const{isOpen:j=!0,setIsOpen:l,handleDragStart:x,router:i,...f}=n,{onCloseClick:u}=$e(),{className:g,...h}=f,b=ee({warn:!1}),C=i??b,p=te({router:C}),F=[...p.pendingMatches??[],...p.matches,...p.cachedMatches];ae(C);const[R,O]=P("tanstackRouterDevtoolsShowMatches",!0),[S,E]=P("tanstackRouterDevtoolsActiveRouteId",""),y=$.useMemo(()=>F.find(c=>c.routeId===S||c.id===S),[F,S]),V=Object.keys(p.location.search||{}).length,L={...C,state:C.state};return e.jsxs("div",{ref:s,className:k(o().devtoolsPanel,"TanStackRouterDevtoolsPanel",g),...h,children:[x?e.jsx("div",{className:o().dragHandle,onMouseDown:x}):null,e.jsx("button",{className:o().panelCloseBtn,onClick:c=>{l(!1),u(c)},children:e.jsx("svg",{xmlns:"http://www.w3.org/2000/svg",width:"10",height:"6",fill:"none",viewBox:"0 0 10 6",className:o().panelCloseBtnIcon,children:e.jsx("path",{stroke:"currentColor",strokeLinecap:"round",strokeLinejoin:"round",strokeWidth:"1.667",d:"M1 1l4 4 4-4"})})}),e.jsxs("div",{className:o().firstContainer,children:[e.jsx("div",{className:o().row,children:e.jsx(je,{"aria-hidden":!0,onClick:c=>{l(!1),u(c)}})}),e.jsx("div",{className:o().routerExplorerContainer,children:e.jsx("div",{className:o().routerExplorer,children:e.jsx(D,{label:"Router",value:Object.fromEntries(pe(Object.keys(L),["state","routesById","routesByPath","flatRoutes","options"].map(c=>N=>N!==c)).map(c=>[c,L[c]]).filter(c=>typeof c[1]!="function"&&!["__store","basepath","injectedHtml","subscribers","latestLoadPromise","navigateTimeout","resetNextScroll","tempLocationKey","latestLocation","routeTree","history"].includes(c[0]))),defaultExpanded:{state:{},context:{},options:{}},filterSubEntries:c=>c.filter(N=>typeof N.value!="function")})})})]}),e.jsxs("div",{className:o().secondContainer,children:[e.jsxs("div",{className:o().matchesContainer,children:[e.jsxs("div",{className:o().detailsHeader,children:[e.jsx("span",{children:"Pathname"}),p.location.maskedLocation?e.jsx("div",{className:o().maskedBadgeContainer,children:e.jsx("span",{className:o().maskedBadge,children:"masked"})}):null]}),e.jsxs("div",{className:o().detailsContent,children:[e.jsx("code",{children:p.location.pathname}),p.location.maskedLocation?e.jsx("code",{className:o().maskedLocation,children:p.location.maskedLocation.pathname}):null]}),e.jsxs("div",{className:o().detailsHeader,children:[e.jsxs("div",{className:o().routeMatchesToggle,children:[e.jsx("button",{type:"button",onClick:()=>{O(!1)},disabled:!R,className:k(o().routeMatchesToggleBtn(!R,!0)),children:"Routes"}),e.jsx("button",{type:"button",onClick:()=>{O(!0)},disabled:R,className:k(o().routeMatchesToggleBtn(!!R,!1)),children:"Matches"})]}),e.jsx("div",{className:o().detailsHeaderInfo,children:e.jsx("div",{children:"age / staleTime / gcTime"})})]}),R?e.jsx("div",{children:(p.status==="pending"?p.pendingMatches??[]:p.matches).map((c,N)=>e.jsxs("div",{role:"button","aria-label":`Open match details for ${c.id}`,onClick:()=>E(S===c.id?"":c.id),className:k(o().matchRow(c===y)),children:[e.jsx("div",{className:k(o().matchIndicator(W(c)))}),e.jsx("code",{className:o().matchID,children:`${c.routeId==="__root__"?"__root__":c.pathname}`}),e.jsx(_,{match:c})]},c.id||N))}):e.jsx(ne,{route:C.routeTree,isRoot:!0,activeId:S,setActiveId:E})]}),(d=p.cachedMatches)!=null&&d.length?e.jsxs("div",{className:o().cachedMatchesContainer,children:[e.jsxs("div",{className:o().detailsHeader,children:[e.jsx("div",{children:"Cached Matches"}),e.jsx("div",{className:o().detailsHeaderInfo,children:"age / staleTime / gcTime"})]}),e.jsx("div",{children:p.cachedMatches.map(c=>e.jsxs("div",{role:"button","aria-label":`Open match details for ${c.id}`,onClick:()=>E(S===c.id?"":c.id),className:k(o().matchRow(c===y)),children:[e.jsx("div",{className:k(o().matchIndicator(W(c)))}),e.jsx("code",{className:o().matchID,children:`${c.id}`}),e.jsx(_,{match:c})]},c.id))})]}):null]}),y?e.jsxs("div",{className:o().thirdContainer,children:[e.jsx("div",{className:o().detailsHeader,children:"Match Details"}),e.jsx("div",{children:e.jsxs("div",{className:o().matchDetails,children:[e.jsx("div",{className:o().matchStatus(y.status,y.isFetching),children:e.jsx("div",{children:y.status==="success"&&y.isFetching?"fetching":y.status})}),e.jsxs("div",{className:o().matchDetailsInfoLabel,children:[e.jsx("div",{children:"ID:"}),e.jsx("div",{className:o().matchDetailsInfo,children:e.jsx("code",{children:y.id})})]}),e.jsxs("div",{className:o().matchDetailsInfoLabel,children:[e.jsx("div",{children:"State:"}),e.jsx("div",{className:o().matchDetailsInfo,children:(m=p.pendingMatches)!=null&&m.find(c=>c.id===y.id)?"Pending":(a=p.matches)!=null&&a.find(c=>c.id===y.id)?"Active":"Cached"})]}),e.jsxs("div",{className:o().matchDetailsInfoLabel,children:[e.jsx("div",{children:"Last Updated:"}),e.jsx("div",{className:o().matchDetailsInfo,children:y.updatedAt?new Date(y.updatedAt).toLocaleTimeString():"N/A"})]})]})}),y.loaderData?e.jsxs(e.Fragment,{children:[e.jsx("div",{className:o().detailsHeader,children:"Loader Data"}),e.jsx("div",{className:o().detailsContent,children:e.jsx(D,{label:"loaderData",value:y.loaderData,defaultExpanded:{}})})]}):null,e.jsx("div",{className:o().detailsHeader,children:"Explorer"}),e.jsx("div",{className:o().detailsContent,children:e.jsx(D,{label:"Match",value:y,defaultExpanded:{}})})]}):null,V?e.jsxs("div",{className:o().fourthContainer,children:[e.jsx("div",{className:o().detailsHeader,children:"Search Params"}),e.jsx("div",{className:o().detailsContent,children:e.jsx(D,{value:p.location.search||{},defaultExpanded:Object.keys(p.location.search||{}).reduce((c,N)=>(c[N]={},c),{})})})]}):null]})});function _({match:t}){const n=ee(),s=$.useReducer(()=>({}),()=>({}))[1];if($.useEffect(()=>{const l=setInterval(()=>{s()},1e3);return()=>{clearInterval(l)}},[]),!t)return null;const d=n.looseRoutesById[t==null?void 0:t.routeId];if(!d.options.loader)return null;const m=Date.now()-(t==null?void 0:t.updatedAt),a=d.options.staleTime??n.options.defaultStaleTime??0,j=d.options.gcTime??n.options.defaultGcTime??30*60*1e3;return e.jsxs("div",{className:k(o().ageTicker(m>a)),children:[e.jsx("div",{children:H(m)}),e.jsx("div",{children:"/"}),e.jsx("div",{children:H(a)}),e.jsx("div",{children:"/"}),e.jsx("div",{children:H(j)})]})}function H(t){const n=["s","min","h","d"],s=[t/1e3,t/6e4,t/36e5,t/864e5];let d=0;for(let a=1;a<s.length&&!(s[a]<1);a++)d=a;return new Intl.NumberFormat(navigator.language,{compactDisplay:"short",notation:"compact",maximumFractionDigits:0}).format(s[d])+n[d]}const be=()=>{const{colors:t,font:n,size:s,alpha:d,shadow:m,border:a}=v,{fontFamily:j,lineHeight:l,size:x}=n;return{devtoolsPanelContainer:r`
      direction: ltr;
      position: fixed;
      bottom: 0;
      right: 0;
      z-index: 99999;
      width: 100%;
      max-height: 90%;
      border-top: 1px solid ${t.gray[700]};
      transform-origin: top;
    `,devtoolsPanelContainerVisibility:i=>r`
        visibility: ${i?"visible":"hidden"};
      `,devtoolsPanelContainerResizing:i=>i?r`
          transition: none;
        `:r`
        transition: all 0.4s ease;
      `,devtoolsPanelContainerAnimation:(i,f)=>i?r`
          pointer-events: auto;
          transform: translateY(0);
        `:r`
        pointer-events: none;
        transform: translateY(${f}px);
      `,logo:r`
      cursor: pointer;
      display: flex;
      flex-direction: column;
      background-color: transparent;
      border: none;
      font-family: ${j.sans};
      gap: ${v.size[.5]};
      padding: 0px;
      &:hover {
        opacity: 0.7;
      }
      &:focus-visible {
        outline-offset: 4px;
        border-radius: ${a.radius.xs};
        outline: 2px solid ${t.blue[800]};
      }
    `,tanstackLogo:r`
      font-size: ${n.size.md};
      font-weight: ${n.weight.bold};
      line-height: ${n.lineHeight.xs};
      white-space: nowrap;
      color: ${t.gray[300]};
    `,routerLogo:r`
      font-weight: ${n.weight.semibold};
      font-size: ${n.size.xs};
      background: linear-gradient(to right, #84cc16, #10b981);
      background-clip: text;
      -webkit-background-clip: text;
      line-height: 1;
      -webkit-text-fill-color: transparent;
      white-space: nowrap;
    `,devtoolsPanel:r`
      display: flex;
      font-size: ${x.sm};
      font-family: ${j.sans};
      background-color: ${t.darkGray[700]};
      color: ${t.gray[300]};

      @media (max-width: 700px) {
        flex-direction: column;
      }
      @media (max-width: 600px) {
        font-size: ${x.xs};
      }
    `,dragHandle:r`
      position: absolute;
      left: 0;
      top: 0;
      width: 100%;
      height: 4px;
      cursor: row-resize;
      z-index: 100000;
      &:hover {
        background-color: ${t.purple[400]}${d[90]};
      }
    `,firstContainer:r`
      flex: 1 1 500px;
      min-height: 40%;
      max-height: 100%;
      overflow: auto;
      border-right: 1px solid ${t.gray[700]};
      display: flex;
      flex-direction: column;
    `,routerExplorerContainer:r`
      overflow-y: auto;
      flex: 1;
    `,routerExplorer:r`
      padding: ${v.size[2]};
    `,row:r`
      display: flex;
      align-items: center;
      padding: ${v.size[2]} ${v.size[2.5]};
      gap: ${v.size[2.5]};
      border-bottom: ${t.darkGray[500]} 1px solid;
      align-items: center;
    `,detailsHeader:r`
      font-family: ui-sans-serif, Inter, system-ui, sans-serif, sans-serif;
      position: sticky;
      top: 0;
      z-index: 2;
      background-color: ${t.darkGray[600]};
      padding: 0px ${v.size[2]};
      font-weight: ${n.weight.medium};
      font-size: ${n.size.xs};
      min-height: ${v.size[8]};
      line-height: ${n.lineHeight.xs};
      text-align: left;
      display: flex;
      align-items: center;
    `,maskedBadge:r`
      background: ${t.yellow[900]}${d[70]};
      color: ${t.yellow[300]};
      display: inline-block;
      padding: ${v.size[0]} ${v.size[2.5]};
      border-radius: ${a.radius.full};
      font-size: ${n.size.xs};
      font-weight: ${n.weight.normal};
      border: 1px solid ${t.yellow[300]};
    `,maskedLocation:r`
      color: ${t.yellow[300]};
    `,detailsContent:r`
      padding: ${v.size[1.5]} ${v.size[2]};
      display: flex;
      align-items: center;
      font-size: ${n.size.xs};
    `,routeMatchesToggle:r`
      display: flex;
      align-items: center;
      border: 1px solid ${t.gray[500]};
      border-radius: ${a.radius.sm};
      overflow: hidden;
    `,routeMatchesToggleBtn:(i,f)=>{const g=[r`
        appearance: none;
        border: none;
        font-size: 12px;
        padding: 4px 8px;
        background: transparent;
        cursor: pointer;
        font-family: ${j.sans};
        font-weight: ${n.weight.medium};
      `];if(i){const h=r`
          background: ${t.darkGray[400]};
          color: ${t.gray[300]};
        `;g.push(h)}else{const h=r`
          color: ${t.gray[500]};
          background: ${t.darkGray[800]}${d[20]};
        `;g.push(h)}if(f){const h=r`
          border-right: 1px solid ${v.colors.gray[500]};
        `;g.push(h)}return g},detailsHeaderInfo:r`
      flex: 1;
      justify-content: flex-end;
      display: flex;
      align-items: center;
      font-weight: ${n.weight.normal};
      color: ${t.gray[400]};
    `,matchRow:i=>{const u=[r`
        display: flex;
        border-bottom: 1px solid ${t.darkGray[400]};
        cursor: pointer;
        align-items: center;
        padding: ${s[1]} ${s[2]};
        gap: ${s[2]};
        font-size: ${x.xs};
        color: ${t.gray[300]};
      `];if(i){const g=r`
          background: ${t.darkGray[500]};
        `;u.push(g)}return u},matchIndicator:i=>{const u=[r`
        flex: 0 0 auto;
        width: ${s[3]};
        height: ${s[3]};
        background: ${t[i][900]};
        border: 1px solid ${t[i][500]};
        border-radius: ${a.radius.full};
        transition: all 0.25s ease-out;
        box-sizing: border-box;
      `];if(i==="gray"){const g=r`
          background: ${t.gray[700]};
          border-color: ${t.gray[400]};
        `;u.push(g)}return u},matchID:r`
      flex: 1;
      line-height: ${l.xs};
    `,ageTicker:i=>{const u=[r`
        display: flex;
        gap: ${s[1]};
        font-size: ${x.xs};
        color: ${t.gray[400]};
        font-variant-numeric: tabular-nums;
        line-height: ${l.xs};
      `];if(i){const g=r`
          color: ${t.yellow[400]};
        `;u.push(g)}return u},secondContainer:r`
      flex: 1 1 500px;
      min-height: 40%;
      max-height: 100%;
      overflow: auto;
      border-right: 1px solid ${t.gray[700]};
      display: flex;
      flex-direction: column;
    `,thirdContainer:r`
      flex: 1 1 500px;
      overflow: auto;
      display: flex;
      flex-direction: column;
      height: 100%;
      border-right: 1px solid ${t.gray[700]};

      @media (max-width: 700px) {
        border-top: 2px solid ${t.gray[700]};
      }
    `,fourthContainer:r`
      flex: 1 1 500px;
      min-height: 40%;
      max-height: 100%;
      overflow: auto;
      display: flex;
      flex-direction: column;
    `,routesRowContainer:(i,f)=>{const g=[r`
        display: flex;
        border-bottom: 1px solid ${t.darkGray[400]};
        align-items: center;
        padding: ${s[1]} ${s[2]};
        gap: ${s[2]};
        font-size: ${x.xs};
        color: ${t.gray[300]};
        cursor: ${f?"pointer":"default"};
        line-height: ${l.xs};
      `];if(i){const h=r`
          background: ${t.darkGray[500]};
        `;g.push(h)}return g},routesRow:i=>{const u=[r`
        flex: 1 0 auto;
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: ${x.xs};
        line-height: ${l.xs};
      `];if(!i){const g=r`
          color: ${t.gray[400]};
        `;u.push(g)}return u},routeParamInfo:r`
      color: ${t.gray[400]};
      font-size: ${x.xs};
      line-height: ${l.xs};
    `,nestedRouteRow:i=>r`
        margin-left: ${i?0:s[3.5]};
        border-left: ${i?"":`solid 1px ${t.gray[700]}`};
      `,code:r`
      font-size: ${x.xs};
      line-height: ${l.xs};
    `,matchesContainer:r`
      flex: 1 1 auto;
      overflow-y: auto;
    `,cachedMatchesContainer:r`
      flex: 1 1 auto;
      overflow-y: auto;
      max-height: 50%;
    `,maskedBadgeContainer:r`
      flex: 1;
      justify-content: flex-end;
      display: flex;
    `,matchDetails:r`
      display: flex;
      flex-direction: column;
      padding: ${v.size[2]};
      font-size: ${v.font.size.xs};
      color: ${v.colors.gray[300]};
      line-height: ${v.font.lineHeight.sm};
    `,matchStatus:(i,f)=>{const g=f&&i==="success"?"blue":{pending:"yellow",success:"green",error:"red"}[i];return r`
        display: flex;
        justify-content: center;
        align-items: center;
        height: 40px;
        border-radius: ${v.border.radius.sm};
        font-weight: ${v.font.weight.normal};
        background-color: ${v.colors[g][900]}${v.alpha[90]};
        color: ${v.colors[g][300]};
        border: 1px solid ${v.colors[g][600]};
        margin-bottom: ${v.size[2]};
        transition: all 0.25s ease-out;
      `},matchDetailsInfo:r`
      display: flex;
      justify-content: flex-end;
      flex: 1;
    `,matchDetailsInfoLabel:r`
      display: flex;
    `,mainCloseBtn:r`
      background: ${t.darkGray[700]};
      padding: ${s[1]} ${s[2]} ${s[1]} ${s[1.5]};
      border-radius: ${a.radius.md};
      position: fixed;
      z-index: 99999;
      display: inline-flex;
      width: fit-content;
      cursor: pointer;
      appearance: none;
      border: 0;
      gap: 8px;
      align-items: center;
      border: 1px solid ${t.gray[500]};
      font-size: ${n.size.xs};
      cursor: pointer;
      transition: all 0.25s ease-out;

      &:hover {
        background: ${t.darkGray[500]};
      }
    `,mainCloseBtnPosition:i=>r`
        ${i==="top-left"?`top: ${s[2]}; left: ${s[2]};`:""}
        ${i==="top-right"?`top: ${s[2]}; right: ${s[2]};`:""}
        ${i==="bottom-left"?`bottom: ${s[2]}; left: ${s[2]};`:""}
        ${i==="bottom-right"?`bottom: ${s[2]}; right: ${s[2]};`:""}
      `,mainCloseBtnAnimation:i=>i?r`
          opacity: 1;
          pointer-events: auto;
          visibility: visible;
        `:r`
        opacity: 0;
        pointer-events: none;
        visibility: hidden;
      `,routerLogoCloseButton:r`
      font-weight: ${n.weight.semibold};
      font-size: ${n.size.xs};
      background: linear-gradient(to right, #98f30c, #00f4a3);
      background-clip: text;
      -webkit-background-clip: text;
      line-height: 1;
      -webkit-text-fill-color: transparent;
      white-space: nowrap;
    `,mainCloseBtnDivider:r`
      width: 1px;
      background: ${v.colors.gray[600]};
      height: 100%;
      border-radius: 999999px;
      color: transparent;
    `,mainCloseBtnIconContainer:r`
      position: relative;
      width: ${s[5]};
      height: ${s[5]};
      background: pink;
      border-radius: 999999px;
      overflow: hidden;
    `,mainCloseBtnIconOuter:r`
      width: ${s[5]};
      height: ${s[5]};
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      filter: blur(3px) saturate(1.8) contrast(2);
    `,mainCloseBtnIconInner:r`
      width: ${s[4]};
      height: ${s[4]};
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
    `,panelCloseBtn:r`
      position: absolute;
      cursor: pointer;
      z-index: 100001;
      display: flex;
      align-items: center;
      justify-content: center;
      outline: none;
      background-color: ${t.darkGray[700]};
      &:hover {
        background-color: ${t.darkGray[500]};
      }

      top: 0;
      right: ${s[2]};
      transform: translate(0, -100%);
      border-right: ${t.darkGray[300]} 1px solid;
      border-left: ${t.darkGray[300]} 1px solid;
      border-top: ${t.darkGray[300]} 1px solid;
      border-bottom: none;
      border-radius: ${a.radius.sm} ${a.radius.sm} 0px 0px;
      padding: ${s[1]} ${s[1.5]} ${s[.5]} ${s[1.5]};

      &::after {
        content: ' ';
        position: absolute;
        top: 100%;
        left: -${s[2.5]};
        height: ${s[1.5]};
        width: calc(100% + ${s[5]});
      }
    `,panelCloseBtnIcon:r`
      color: ${t.gray[400]};
      width: ${s[2]};
      height: ${s[2]};
    `}};let A=null;function o(){return A||(A=be(),A)}export{Ce as TanStackRouterDevtools,ye as TanStackRouterDevtoolsPanel};
