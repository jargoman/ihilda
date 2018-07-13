using System;
using RippleLibSharp.Binary;

namespace RippleLibSharp.Nodes
{
	public class RippleNodeGroup
	{
		/*
		public RippleNodeGroup ()
		{
		}
		*/

		public RippleNode CreatedNode {
			get; /*{ return node; }*/
			set; /*{ this.type = getType ();
				this.node = getnode ();
			}*/
		}
		public RippleNode ModifiedNode {
			get; /*{ return node; }*/
			set; /*{ this.type = getType ();this.node = getnode ();}*/
		}

		public RippleNode DeletedNode {
			get; /*{ return node; }*/
			set; /*{ this.type = getType ();this.node = getnode ();}*/
		}

		/*
		public RippleNode node {
			get;
			set;
		}
		*/

		//public BinaryFieldType type = null;

		public RippleNode GetNode () {
			if (CreatedNode != null) {
				CreatedNode.nodeType = BinaryFieldType.CreatedNode;
				return CreatedNode;
			}

			if (ModifiedNode != null) {
				ModifiedNode.nodeType = BinaryFieldType.ModifiedNode;
				return ModifiedNode;
			}

			if (DeletedNode != null) {
				DeletedNode.nodeType = BinaryFieldType.DeletedNode;
				return DeletedNode;
			}

			return null;
		}

		public BinaryFieldType GetNodeType () {
			if (CreatedNode != null) {
				return BinaryFieldType.CreatedNode;
			}

			if (ModifiedNode != null) {
				return BinaryFieldType.ModifiedNode;
			}

			if (DeletedNode != null) {
				return BinaryFieldType.DeletedNode;
			}

			return null;
		}
	}
}

