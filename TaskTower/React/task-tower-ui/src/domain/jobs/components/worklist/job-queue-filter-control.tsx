import { PlusCircleIcon } from "lucide-react";
import * as React from "react";

import { BadgeAvatar } from "@/components/ui/badge-avatar";
import { Button } from "@/components/ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from "@/components/ui/command";
import { cn } from "@/utils";
import { Popover, PopoverContent, PopoverTrigger } from "@nextui-org/react";
import { CheckIcon } from "lucide-react";
import { useJobsTableStore } from "./jobs-worklist.store";

interface FilterControl {
  title?: string;
  options: {
    label: string;
    value: string;
  }[];
}

export function QueueFilterControl({ title, options }: FilterControl) {
  const { addQueue, removeQueue, queue, clearQueue } = useJobsTableStore();
  const selectedValues = new Set(queue);
  const [popoverIsOpen, setPopoverIsOpen] = React.useState(false);

  return (
    <Popover
      placement="bottom-start"
      isOpen={popoverIsOpen}
      onOpenChange={(open) => setPopoverIsOpen(open)}
    >
      <PopoverTrigger>
        <Button variant="outline" className="border-dashed">
          <PlusCircleIcon className="w-4 h-4 mr-2" />
          {title}
          {selectedValues?.size > 0 && (
            <>
              <div className="h-4 border-l border-gray-300 mx-2" />
              <BadgeAvatar
                variant="secondary"
                className="px-1 font-normal rounded-sm lg:hidden"
              >
                {selectedValues.size}
              </BadgeAvatar>
              <div className="hidden space-x-1 lg:flex">
                {selectedValues.size > 2 ? (
                  <BadgeAvatar
                    variant="secondary"
                    className="px-1 font-normal rounded-sm"
                  >
                    {selectedValues.size} selected
                  </BadgeAvatar>
                ) : (
                  options
                    .filter((option) => selectedValues.has(option.value))
                    .map((option) => (
                      <BadgeAvatar
                        variant="secondary"
                        key={option.value}
                        className="px-1 font-normal rounded-sm"
                      >
                        {option.label}
                      </BadgeAvatar>
                    ))
                )}
              </div>
            </>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[12rem] p-0">
        <Command>
          <CommandInput placeholder={title} />
          <CommandList>
            <CommandEmpty>No results found.</CommandEmpty>
            <CommandGroup>
              {options.map((option) => {
                const isSelected = selectedValues.has(option.value);
                return (
                  <CommandItem
                    key={option.value}
                    onSelect={() => {
                      if (isSelected) {
                        removeQueue(option.value);
                        selectedValues.delete(option.value);
                      } else {
                        addQueue(option.value);
                        selectedValues.add(option.value);
                      }
                    }}
                  >
                    <div
                      className={cn(
                        "mr-2 flex h-4 w-4 items-center justify-center rounded-sm border border-emerald-500",
                        isSelected
                          ? "bg-emerald-500 text-white"
                          : "opacity-50 [&_svg]:invisible"
                      )}
                    >
                      <CheckIcon className={cn("h-4 w-4")} />
                    </div>
                    <span>{option.label}</span>
                  </CommandItem>
                );
              })}
            </CommandGroup>
            {selectedValues.size > 0 && (
              <>
                <CommandSeparator />
                <CommandGroup>
                  <CommandItem
                    onSelect={() => {
                      clearQueue();
                      setPopoverIsOpen(false);
                    }}
                    className="justify-center text-center"
                  >
                    Clear filters
                  </CommandItem>
                </CommandGroup>
              </>
            )}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
