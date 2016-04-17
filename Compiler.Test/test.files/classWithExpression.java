class calculator {
    int x, y, z;

    public void test1(){
        int ans; 
        ans = 3 * 5 + 8 / 9;
        return ;
    }

    public int sum(int a, int b){
        int ans;
        ans = a + b;
        a = a*4;
        b = -b/a;
        return ans;
    }

    public int test2(int a){
        return -a + 27;
    }
        
    public int test3(int a, int b){
        int ans; 
        ans = calculator.sum(a, b);
        calculator.sum(a, b);
        return ans;
    }

}

final class Main {
    public static void main(String[] args){
        calculator.test2(1,2);
    }
}