using System;
using System.Collections.Generic;

namespace SimpleInventoryManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            Inventory inventory = new Inventory();
            string choice;

            do
            {
                Console.WriteLine("\nInventory Management System");
                Console.WriteLine("1. Add Item");
                Console.WriteLine("2. Display All Items");
                Console.WriteLine("3. Find Item by ID");
                Console.WriteLine("4. Update Item");
                Console.WriteLine("5. Delete Item");
                Console.WriteLine("6. Exit");
                Console.Write("Select an option: ");
                choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddItem(inventory);
                        break;
                    case "2":
                        inventory.DisplayAllItems();
                        break;
                    case "3":
                        FindItemByID(inventory);
                        break;
                    case "4":
                        UpdateItem(inventory);
                        break;
                    case "5":
                        DeleteItem(inventory);
                        break;
                    case "6":
                        Console.WriteLine("Exiting the system.");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            } while (choice != "6");
        }

        static void AddItem(Inventory inventory)
        {
            try
            {
                Console.Write("Enter Item ID: ");
                int id = int.Parse(Console.ReadLine());
                Console.Write("Enter Item Name: ");
                string name = Console.ReadLine();
                Console.Write("Enter Item Price: ");
                double price = double.Parse(Console.ReadLine());
                Console.Write("Enter Item Quantity: ");
                int quantity = int.Parse(Console.ReadLine());

                inventory.AddItem(new Item(id, name, price, quantity));
                Console.WriteLine("Item added successfully.");
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid input. Please enter correct data types.");
            }
        }

        static void FindItemByID(Inventory inventory)
        {
            try
            {
                Console.Write("Enter Item ID to find: ");
                int id = int.Parse(Console.ReadLine());
                Item item = inventory.FindItemByID(id);

                if (item != null)
                {
                    Console.WriteLine(item);
                }
                else
                {
                    Console.WriteLine("Item not found.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid input. Please enter a valid ID.");
            }
        }

        static void UpdateItem(Inventory inventory)
        {
            try
            {
                Console.Write("Enter Item ID to update: ");
                int id = int.Parse(Console.ReadLine());
                Item item = inventory.FindItemByID(id);

                if (item != null)
                {
                    Console.Write("Enter new Name: ");
                    item.Name = Console.ReadLine();
                    Console.Write("Enter new Price: ");
                    item.Price = double.Parse(Console.ReadLine());
                    Console.Write("Enter new Quantity: ");
                    item.Quantity = int.Parse(Console.ReadLine());

                    Console.WriteLine("Item updated successfully.");
                }
                else
                {
                    Console.WriteLine("Item not found.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid input. Please enter correct data types.");
            }
        }

        static void DeleteItem(Inventory inventory)
        {
            try
            {
                Console.Write("Enter Item ID to delete: ");
                int id = int.Parse(Console.ReadLine());
                if (inventory.DeleteItem(id))
                {
                    Console.WriteLine("Item deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Item not found.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid input. Please enter a valid ID.");
            }
        }
    }

    class Item
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public Item(int id, string name, double price, int quantity)
        {
            ID = id;
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return $"ID: {ID}, Name: {Name}, Price: {Price}, Quantity: {Quantity}";
        }
    }

    class Inventory
    {
        private List<Item> items = new List<Item>();

        public void AddItem(Item item)
        {
            items.Add(item);
        }

        public void DisplayAllItems()
        {
            if (items.Count == 0)
            {
                Console.WriteLine("No items in the inventory.");
            }
            else
            {
                foreach (var item in items)
                {
                    Console.WriteLine(item);
                }
            }
        }

        public Item FindItemByID(int id)
        {
            return items.Find(item => item.ID == id);
        }

        public bool DeleteItem(int id)
        {
            Item item = FindItemByID(id);
            if (item != null)
            {
                items.Remove(item);
                return true;
            }
            return false;
        }
    }
}
