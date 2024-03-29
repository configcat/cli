using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Rendering;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class Cat(IOutput output)
{
    public Task<int> InvokeAsync()
    {
        output.WriteLine();
        output.WriteLine("                                                              oK");
        output.WriteLine("                                                             koK");
        output.WriteLine("                                                           Xk;oK");
        output.WriteLine("                                                         Xkl;:oK");
        output.WriteLine("      No                                               Xkl;;;:oK");
        output.WriteLine("      N0X                                            Xkl;;;;;:oK");
        output.WriteLine("      Ko;xK                                        Xkl;;;;;;;:oK");
        output.WriteLine("      Ko;;cxK                                    Xkl;;;;;;;;;:oK");
        output.WriteLine("      Kl;;;;cxK                                Xkl;;;;;;;;;;;:oK");
        output.WriteLine("      Kl;;;;;;cxK                           Xkdl;;;;;;;;;;;;;:oK");
        output.WriteLine("      Kl;;;;;;;;cxK                       Xkl;;;;;;;;;;;;;;;;:oK");
        output.WriteLine("      Kl;;;;;;;;;;cxK                   Xkl;;;;;;;;;;;;;;;;;;:oK");
        output.WriteLine("      Kl;;;;;;;;;;;;cdxxxxxxxxxxxxxxxxxdl;;;;;;;;;;;;;;;;;;;;:lK");
        output.WriteLine("      Kl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:lK");
        output.WriteLine("      Kl;;;;;;;;;;;;:::::::::::::::::::::::::::::;;;;;;;;;;;;:lK");
        output.WriteLine("      Kl;;;;;;;;cdkO000000000KK00000KK000K00000000Oxl:;;;;;;;:lK                www");
        output.WriteLine("      Kl;;;;;;cxXNNXOkk0kkOKNWMMMMMMMMMN0dc ``` okXWNOo;;;;;;:lK             N0kddodxOX");
        output.WriteLine("      Kl;;;;;cOWWKxok0KXK0kxx0WMMMMMMW0            xNWKo;;;;;:lK           NOl:;;;;;;;cxX");
        output.WriteLine("      Kl;;;;;dNWKoxXWMMMMMMNkoOWMMMMMO              dNWOc;;;;:lK          Nk:;;;;;;;;;;;oK");
        output.WriteLine("      Kl;;;;;xNNxoKWMMMMMMMMNxdXMMMMWl               KW0c;;;;:lK          Kl;;;;;;;;;;;;:dN");
        output.WriteLine("      Kl;;;;;xNNkoKWMMMMMMMMXddXMMMMWo               KW0c;;;;:lK          Ko;;;;;;;;;;;;;c0");
        output.WriteLine("      Kl;;;;;dXWKddKWMMMMMWXxo0WMMMMMK              kWWk:;;;;:lK           k:;;;;;;;;;;;;:xN");
        output.WriteLine("      Kl;;;;;:xNWXkoxO000OkokKWMMMMMMMXd          c0WW0l;;;;;:lK           Xo;;;;;;;;;;;;;oX");
        output.WriteLine("      Kl;;;;;;:oOXNX0Ok0kk0XWMMMMMMMMMMWNOdollodkXNNKxc;;;;;;:lK            k:;;;;;;;;;;;;oX");
        output.WriteLine("      Kl;;;;;;;;:loxkkkkkkkkkkkkkkkkkkkkkkkkkkkkkxdoc;;;;;;;;:lK            0c;;;;;;;;;;;;oX");
        output.WriteLine("      Kl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:lK            0c;;;;;;;;;;;;dN");
        output.WriteLine("      Kl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:lK            Oc;;;;;;;;;;;:kk");
        output.WriteLine("  oOdddddddddddddc:;;;;;;;;;;;dd;;;;;bb;;;;;;;;;;:cdddddddddddddddddOo     Nx:;;;;;;;;;;;ln");
        output.WriteLine("      NOxxxxxxxx:;;;;;;;;;;;;;;;dO;Ob;;;;;;;;;;;;;;;:xxxxxxxxxKK           Kl;;;;;;;;;;;:kN");
        output.WriteLine("oOdddddddddddddddddddoc:;;;;;;;;;;0;;;;;;;;;;:clddddddddddddddddddddddOo  Nx:;;;;;;;;;;:xN");
        output.WriteLine("       Xkxxxxxx:;;;;;;;;;;;;;;;;;;0;;;;;;;;;;;;;;;;;;:xxxxxxxxKK         Nk:;;;;;;;;;;:dX");
        output.WriteLine("  oOddddddoooooooooc:;;;;;;;;;;;;;o;;;;;;;;;;;;;;:ccccccccccclddddddOo  Xx:;;;;;;;;;;:xX");
        output.WriteLine("        Xd;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:kK       0o:;;;;;;;;;;lON");
        output.WriteLine("         Xd:;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:kK     Kxc;;;;;;;;;;cxK");
        output.WriteLine("          NOl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:kK  N0dc;;;;;;;;;;cd0N");
        output.WriteLine("            Xkl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:xX0xo:;;;;;;;;;:lxK");
        output.WriteLine("              XOoc;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;cc:;;;;;;;;cox0N");
        output.WriteLine("                 KOxoc:;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;cloxOKN");
        output.WriteLine("                      cccdxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxbl");
        output.WriteLine();

        return Task.FromResult(ExitCodes.Ok);
    }
}