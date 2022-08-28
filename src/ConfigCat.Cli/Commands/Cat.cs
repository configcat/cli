using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Rendering;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class Cat
{
    private readonly IOutput output;

    public Cat(IOutput output)
    {
        this.output = output;
    }

    public Task<int> InvokeAsync()
    {
        this.output.WriteLine();
        this.output.WriteLine("                                                              oK");
        this.output.WriteLine("                                                             koK");
        this.output.WriteLine("                                                           Xk;oK");
        this.output.WriteLine("                                                         Xkl;:oK");
        this.output.WriteLine("      No                                               Xkl;;;:oK");
        this.output.WriteLine("      N0X                                            Xkl;;;;;:oK");
        this.output.WriteLine("      Ko;xK                                        Xkl;;;;;;;:oK");
        this.output.WriteLine("      Ko;;cxK                                    Xkl;;;;;;;;;:oK");
        this.output.WriteLine("      Kl;;;;cxK                                Xkl;;;;;;;;;;;:oK");
        this.output.WriteLine("      Kl;;;;;;cxK                           Xkdl;;;;;;;;;;;;;:oK");
        this.output.WriteLine("      Kl;;;;;;;;cxK                       Xkl;;;;;;;;;;;;;;;;:oK");
        this.output.WriteLine("      Kl;;;;;;;;;;cxK                   Xkl;;;;;;;;;;;;;;;;;;:oK");
        this.output.WriteLine("      Kl;;;;;;;;;;;;cdxxxxxxxxxxxxxxxxxdl;;;;;;;;;;;;;;;;;;;;:lK");
        this.output.WriteLine("      Kl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:lK");
        this.output.WriteLine("      Kl;;;;;;;;;;;;:::::::::::::::::::::::::::::;;;;;;;;;;;;:lK");
        this.output.WriteLine("      Kl;;;;;;;;cdkO000000000KK00000KK000K00000000Oxl:;;;;;;;:lK                www");
        this.output.WriteLine("      Kl;;;;;;cxXNNXOkk0kkOKNWMMMMMMMMMN0dc ``` okXWNOo;;;;;;:lK             N0kddodxOX");
        this.output.WriteLine("      Kl;;;;;cOWWKxok0KXK0kxx0WMMMMMMW0            xNWKo;;;;;:lK           NOl:;;;;;;;cxX");
        this.output.WriteLine("      Kl;;;;;dNWKoxXWMMMMMMNkoOWMMMMMO              dNWOc;;;;:lK          Nk:;;;;;;;;;;;oK");
        this.output.WriteLine("      Kl;;;;;xNNxoKWMMMMMMMMNxdXMMMMWl               KW0c;;;;:lK          Kl;;;;;;;;;;;;:dN");
        this.output.WriteLine("      Kl;;;;;xNNkoKWMMMMMMMMXddXMMMMWo               KW0c;;;;:lK          Ko;;;;;;;;;;;;;c0");
        this.output.WriteLine("      Kl;;;;;dXWKddKWMMMMMWXxo0WMMMMMK              kWWk:;;;;:lK           k:;;;;;;;;;;;;:xN");
        this.output.WriteLine("      Kl;;;;;:xNWXkoxO000OkokKWMMMMMMMXd          c0WW0l;;;;;:lK           Xo;;;;;;;;;;;;;oX");
        this.output.WriteLine("      Kl;;;;;;:oOXNX0Ok0kk0XWMMMMMMMMMMWNOdollodkXNNKxc;;;;;;:lK            k:;;;;;;;;;;;;oX");
        this.output.WriteLine("      Kl;;;;;;;;:loxkkkkkkkkkkkkkkkkkkkkkkkkkkkkkxdoc;;;;;;;;:lK            0c;;;;;;;;;;;;oX");
        this.output.WriteLine("      Kl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:lK            0c;;;;;;;;;;;;dN");
        this.output.WriteLine("      Kl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:lK            Oc;;;;;;;;;;;:kk");
        this.output.WriteLine("  oOdddddddddddddc:;;;;;;;;;;;dd;;;;;bb;;;;;;;;;;:cdddddddddddddddddOo     Nx:;;;;;;;;;;;ln");
        this.output.WriteLine("      NOxxxxxxxx:;;;;;;;;;;;;;;;dO;Ob;;;;;;;;;;;;;;;:xxxxxxxxxKK           Kl;;;;;;;;;;;:kN");
        this.output.WriteLine("oOdddddddddddddddddddoc:;;;;;;;;;;0;;;;;;;;;;:clddddddddddddddddddddddOo  Nx:;;;;;;;;;;:xN");
        this.output.WriteLine("       Xkxxxxxx:;;;;;;;;;;;;;;;;;;0;;;;;;;;;;;;;;;;;;:xxxxxxxxKK         Nk:;;;;;;;;;;:dX");
        this.output.WriteLine("  oOddddddoooooooooc:;;;;;;;;;;;;;o;;;;;;;;;;;;;;:ccccccccccclddddddOo  Xx:;;;;;;;;;;:xX");
        this.output.WriteLine("        Xd;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:kK       0o:;;;;;;;;;;lON");
        this.output.WriteLine("         Xd:;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:kK     Kxc;;;;;;;;;;cxK");
        this.output.WriteLine("          NOl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:kK  N0dc;;;;;;;;;;cd0N");
        this.output.WriteLine("            Xkl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:xX0xo:;;;;;;;;;:lxK");
        this.output.WriteLine("              XOoc;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;cc:;;;;;;;;cox0N");
        this.output.WriteLine("                 KOxoc:;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;cloxOKN");
        this.output.WriteLine("                      cccdxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxbl");
        this.output.WriteLine();

        return Task.FromResult(ExitCodes.Ok);
    }
}