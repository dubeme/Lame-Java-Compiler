class classone{
  public void firstclass(){
    return;
  }
  public int secondclass(){
    int a,b,c,d;

    write("Enter a number");
    read(a);
    b=10;
    d=20;
    c=d+a*b;
    write("The answer is ");
    writeln(c);
    return c;
  }
}
final class Main{
  public static void main(String [] args){
    classone.secondclass();
  }
}
