using RedTeam.Extensions.CodeGenerators;
using RedTeam.Extensions.CodeGenerators.Models;


namespace RedTeam.Extensions.Console.CodeGenerators.Models
{


    public class SubCommandTreeNode : TreeNode<SubCommandTreeNode>
    {
        public string CommandName { get; init; } = null!;
        public string? CommandDescription { get; set; }

        public string? ParameterTypeName { get; set; }

        public SubCommandTreeNode(params SubCommandTreeNode[] nodes) : base(nodes.ToArray())
        {
            foreach (var node in nodes)
            {
                node.Parent = node;
            }
        }

        public override void AddChild(SubCommandTreeNode node)
        {
            base.AddChild(node);
            node.Parent = this;
        }

        public override string ToString()
        {
            var commandName = $"{CommandName}";
            if (Parent != null)
            {
                return $"{commandName}-> {Parent.CommandName}";

            }
            return commandName;
        }
    }



    internal class SubCommandTree
    {

        class DefaultComparer : IEqualityComparer<SubCommandRegistration>
        {
            public bool Equals(SubCommandRegistration x, SubCommandRegistration y)
            {
                return x.CommandName.Equals(y.CommandName) && x.CommandDescription.Equals(y.CommandDescription);
            }

            public int GetHashCode(SubCommandRegistration obj)
            {
                return obj.CommandName.GetHashCode() ^ obj.CommandDescription.GetHashCode();
            }
        }


        private readonly SubCommandTreeNode _root;

        public IEnumerable<SubCommandHandlerRegistration> Registrations { get; }

        public SubCommandTree(IEnumerable<SubCommandHandlerRegistration> registrations, IEnumerable<SubCommandRegistration> subCommands)
        {

            var distinctCommands = subCommands.Distinct(new DefaultComparer());

            _root = new SubCommandTreeNode()
            {
                CommandName = "app"
            };

            foreach (var subCommand in distinctCommands)
            {
                var existingNodes = _root.DescendFirstMatches(x => x.CommandName == subCommand.ParentCommand);
                if (!existingNodes.Any())
                {
                    var commandNode = new SubCommandTreeNode
                    {
                        CommandName = subCommand.CommandName,
                        CommandDescription = subCommand.CommandDescription,


                    };

                    _root.AddChild(commandNode);
                }
                else
                {
                    var node = existingNodes.FirstOrDefault();
                    var commandNode = new SubCommandTreeNode
                    {
                        CommandName = subCommand.CommandName,
                        CommandDescription = subCommand.CommandDescription,

                    };
                    node.AddChild(commandNode);
                }
            }

            Registrations = registrations;

        }

        private void Traversal(IndentedStringBuilder builder, SubCommandTreeNode node)


        {
            if( node == null || builder == null)
            {
                return;
            }

            if (node.Children.Any())
            {
                BeginSubCommandHandlerDeclaration(builder, node!.Parent?.CommandName ?? node!.CommandName , node!.CommandName);
                builder.IncrementIndent();
                foreach (var child in node.Children)
                {
                    Traversal(builder, child);
                }
                foreach (var child in Registrations.Where(x => x.ParentCommandName == node.CommandName))
                {
                    ApppendCommandHandlerDeclarations(builder, child.CommandParametersClassName, child.ParentCommandName, child.CommandName, child.CommandDescription);
                }
                builder.DecrementIndent();
                CloseSubCommandHandlerDeclaration(builder, node.CommandDescription);
            }
            else
            {

                //find matching registrations
                BeginSubCommandHandlerDeclaration(builder, node?.Parent?.CommandName ?? node!.CommandName, node!.CommandName);
                builder.IncrementIndent();
                foreach (var child in Registrations.Where(x => x.ParentCommandName == node.CommandName))
                {
                    ApppendCommandHandlerDeclarations(builder, child.CommandParametersClassName, child.ParentCommandName, child.CommandName, child.CommandDescription);
                }
                builder.DecrementIndent();
                CloseSubCommandHandlerDeclaration(builder, node.CommandDescription);



            }

        }

        public void GenerateSubCommandCodeRegistrations(IndentedStringBuilder builder)
        {

            foreach (var child in _root.Children)
            {
                Traversal(builder, child);
            }

        }
        static void ApppendCommandHandlerDeclarations(IndentedStringBuilder builder, string parameterTypeNane, string? parent, string command, string description)
        {
            builder.AppendLine($"{parent ?? "app"}.AddCommand<{parameterTypeNane}>(\"{command}\",\"{description}\");");
        }
        static void BeginSubCommandHandlerDeclaration(IndentedStringBuilder builder, string? parent, string commandName)
        {
            builder.AppendLine($"{parent ?? "app"}.AddSubCommand(\"{commandName}\", {commandName}=> {{");
        }
        static void CloseSubCommandHandlerDeclaration(IndentedStringBuilder builder, string? parentDescription)
        {
            if (!string.IsNullOrWhiteSpace(parentDescription))
            {
                builder.AppendLine($"}}).WithDescription(\"{parentDescription}\");");
            }
            else
            {
                builder.AppendLine($"}});");
            }
        }
    }

}
