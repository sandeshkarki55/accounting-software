import React from 'react';
import { ActionIcon, Menu } from '@mantine/core';
import { IconDotsVertical, IconEdit, IconTrash } from '@tabler/icons-react';

export interface ActionMenuItem {
  label: string;
  icon: React.ReactNode;
  color?: string;
  onClick: () => void;
}

interface ActionMenuProps {
  items?: ActionMenuItem[];
  onEdit?: () => void;
  onDelete?: () => void;
  menuWidth?: number;
}

const ActionMenu: React.FC<ActionMenuProps> = ({ items, onEdit, onDelete, menuWidth = 120 }) => {
  const menuItems: ActionMenuItem[] = items ?? [
    ...(onEdit ? [{ label: 'Edit', icon: <IconEdit size="1rem" />, onClick: onEdit }] : []),
    ...(onDelete ? [{ label: 'Delete', icon: <IconTrash size="1rem" />, color: 'red', onClick: onDelete }] : []),
  ];

  return (
    <Menu shadow="md" width={menuWidth}>
      <Menu.Target>
        <ActionIcon variant="subtle" color="gray">
          <IconDotsVertical size="1rem" />
        </ActionIcon>
      </Menu.Target>
      <Menu.Dropdown>
        {menuItems.map((item) => (
          <Menu.Item key={item.label} leftSection={item.icon} color={item.color} onClick={item.onClick}>
            {item.label}
          </Menu.Item>
        ))}
      </Menu.Dropdown>
    </Menu>
  );
};

export default ActionMenu;
