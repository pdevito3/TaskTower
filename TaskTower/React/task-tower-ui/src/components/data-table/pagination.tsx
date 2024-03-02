// import { Button } from "@/components/ui/button";
// import { Command, CommandGroup, CommandItem } from "@/components/ui/command";
// import { cn } from "@/lib/utils";
// import { Pagination } from "@/types/apis";
// import { Popover, PopoverContent, PopoverTrigger } from "@nextui-org/react";
// import {
//   ArrowLeftToLine,
//   ArrowRightFromLine,
//   Check,
//   ChevronLeftIcon,
//   ChevronRightIcon,
//   ChevronsUpDown,
// } from "lucide-react";
// import { useState } from "react";

// interface PaginationControlsProps {
//   entityPlural: string;
//   pageNumber: number;
//   apiPagination: Pagination | undefined;
//   pageSize: number;
//   setPageSize: (size: number) => void;
//   setPageNumber: (page: number) => void;
//   className?: string;
// }

// const PageSizeOptions = [1, 10, 20, 30, 40, 50] as const;
// // type PageSize = (typeof PageSizeOptions)[number];
// export function PaginationControls({
//   entityPlural,
//   pageNumber,
//   apiPagination,
//   pageSize,
//   setPageSize,
//   setPageNumber,
//   className,
// }: PaginationControlsProps) {
//   const pageInfo = `${pageNumber} ${
//     apiPagination?.totalPages ? `of ${apiPagination?.totalPages}` : null
//   }`;
//   return (
//     <div
//       className={cn(
//         "flex items-center justify-between px-3 py-2 bg-white dark:bg-slate-700 sm:rounded-b-lg",
//         className
//       )}
//       aria-label={`Table navigation for ${entityPlural} table`}
//     >
//       <div className="flex items-center flex-1 space-x-5">
//         <span className="flex text-sm font-normal text-slate-500 dark:text-slate-400 min-w-[4rem]">
//           <div>Page</div>
//           <span className="pl-1 font-semibold text-slate-900 dark:text-white">
//             {pageInfo}
//           </span>
//         </span>

//         {pageSize !== undefined && (
//           <div className="hidden w-32 sm:block">
//             <PaginationCombobox
//               value={pageSize.toString()}
//               onValueChange={(value) => {
//                 setPageSize(Number(value));
//                 setPageNumber(1);
//               }}
//             />
//           </div>
//         )}
//       </div>

//       <div className="inline-flex items-center -space-x-[2px]">
//         <Button
//           aria-label="First page"
//           variant="outline"
//           className={cn("rounded-r-none")}
//           onClick={() => setPageNumber(1)}
//           disabled={!apiPagination?.hasPrevious}
//         >
//           {<ArrowLeftToLine className="w-5 h-5" />}
//         </Button>
//         <Button
//           aria-label="Previous page"
//           variant="outline"
//           className={cn("rounded-none")}
//           onClick={() =>
//             setPageNumber(
//               apiPagination?.pageNumber ? apiPagination?.pageNumber - 1 : 1
//             )
//           }
//           disabled={!apiPagination?.hasPrevious}
//         >
//           {<ChevronLeftIcon className="w-5 h-5" />}
//         </Button>
//         <Button
//           aria-label="Next page"
//           variant="outline"
//           className={cn("rounded-none")}
//           onClick={() =>
//             setPageNumber(
//               apiPagination?.pageNumber ? apiPagination?.pageNumber + 1 : 1
//             )
//           }
//           disabled={!apiPagination?.hasNext}
//         >
//           {<ChevronRightIcon className="w-5 h-5" />}
//         </Button>
//         <Button
//           aria-label="Last page"
//           variant="outline"
//           className={cn("rounded-l-none")}
//           onClick={() =>
//             setPageNumber(
//               apiPagination?.totalPages ? apiPagination?.totalPages : 1
//             )
//           }
//           disabled={!apiPagination?.hasNext}
//         >
//           {<ArrowRightFromLine className="w-5 h-5" />}
//         </Button>
//       </div>
//     </div>
//   );
// }

// function PaginationCombobox({
//   value,
//   onValueChange,
// }: {
//   value: string;
//   onValueChange: (value: string) => void;
// }) {
//   const [open, setOpen] = useState(false);
//   const pageSizes = PageSizeOptions.map((selectedPageSize) => ({
//     value: selectedPageSize.toString(),
//     label: `Show ${selectedPageSize}`,
//   }));
//   return (
//     <Popover
//       placement="bottom"
//       isOpen={open}
//       onOpenChange={setOpen}
//       triggerScaleOnOpen={false}
//     >
//       <PopoverTrigger>
//         <Button
//           variant="outline"
//           role="combobox"
//           aria-expanded={open}
//           className="w-[150px] justify-between"
//         >
//           {value
//             ? pageSizes.find((pageSize) => pageSize.value === value)?.label
//             : "Select page size..."}
//           <ChevronsUpDown className="w-4 h-4 ml-2 opacity-50 shrink-0" />
//         </Button>
//       </PopoverTrigger>
//       <PopoverContent className="w-[150px] p-0">
//         <Command>
//           <CommandGroup>
//             {pageSizes.map((pageSize) => (
//               <CommandItem
//                 key={pageSize.value}
//                 onSelect={() => {
//                   onValueChange(pageSize.value === value ? "" : pageSize.value);
//                   setOpen(false);
//                 }}
//               >
//                 <Check
//                   className={cn(
//                     "mr-2 h-4 w-4",
//                     value === pageSize.value ? "opacity-100" : "opacity-0"
//                   )}
//                 />
//                 {pageSize.label}
//               </CommandItem>
//             ))}
//           </CommandGroup>
//         </Command>
//       </PopoverContent>
//     </Popover>
//   );
// }
