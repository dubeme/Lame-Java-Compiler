using Compiler.Services;
using System;

namespace Compiler
{
    internal class Program
    {
        private static string justMain = @"
            final class Main {
                public static void main(String[] args){
                }
            }";

        private static string main1Class = @"
            class tim {
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private static string main2Classes = @"
            class tim {
            }
            class tom {
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private static string classWithFields = @"
            class tim {
                int x, y, z;
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private static string classWithFieldsAndMethod = @"
            class tim {
                int x, y, z;
                public void ass(int a, int b, int c){
                    return;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private static string classWithFieldsAndMethods = @"
            class tim {
                int x, y, z;
                boolean a, b, c;
                public void ass(){
                    return;
                }
                public int kit(){
                    return ;
                }
                public boolean dim(){
                    return ;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private static string classWithFieldsMethodsParameters = @"
            class tim {
                int x, y, z;
                boolean a, b, c;
                public void ass(){
                    return;
                }
                public int kit(int x, int y, boolean z){
                    return ;
                }
                public boolean sum(float num, int ans){
                    return ;
                }
                public float jamb(float num){
                    return ;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private static string classWithConstant = @"
            class two{
                final boolean BOOL = false;
                final int NUMBER = 1234567890;
                final float PI = 3.14;
            }
            final class Main {
                public static void main(String[] args) {
                }
            }";

        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Invalid usage - Usage EXECUTABLE JAVA_FILE");
                return;
            }

            var compiler = new CompilerService();
            //compiler.Compile(args[0]);


            CompilerService.Test(justMain);
            Console.WriteLine($"\n\nAll done.\nPlease press enter to continue ... ");
            Console.ReadLine();
        }
    }
}