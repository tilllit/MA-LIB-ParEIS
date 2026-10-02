----------------------------------------------------------------------------------
-- Company:     AddCycle GmbH
-- Engineer:    Till Körsmeier
-- 
-- Create Date: 04.06.2026 14:01:43
-- Design Name: 
-- Module Name: Acquisition_Core - Behavioral
-- Project Name: 
-- Target Devices: 
-- Tool Versions: 
-- Description: 
-- 
-- Dependencies: 
-- 
-- Revision:
-- Revision 0.01 - File Created
-- Additional Comments:
-- 
----------------------------------------------------------------------------------


library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

-- Uncomment the following library declaration if using
-- arithmetic functions with Signed or Unsigned values
--use IEEE.NUMERIC_STD.ALL;

-- Uncomment the following library declaration if instantiating
-- any Xilinx leaf cells in this code.
--library UNISIM;
--use UNISIM.VComponents.all;




---- #########################################
---- ###############  ENTITY  ################
---- #########################################


entity Acquisition_Core is
    generic(
        -- What is actualy needed?
        G_CLK_HZ         : integer := 100_000_000;
        G_SERIAL_CLK_HZ  : integer := 100_000;--1_000_000;
        G_SAMPLE_RATE_HZ : integer := 5;--10_000;
        G_NUM_CARDS      : integer := 30;
        G_BITS_PER_CARD  : integer := 64;--60;
        G_AXIS_BITS      : integer := 64
    );
    port(
        clk_sys : in  std_logic;
        rst_n   : in  std_logic;
    
        -- Signals extracted from AXI4-Lite
        start       : in  std_logic;
        num_samples : in  unsigned(31 downto 0);

        -- Runtime configuration from AXI4-Lite
        -- serial_half_period_clks = clk_sys / (2 * serial_frequency)
        -- sample_period_clks      = clk_sys / sample_frequency
        -- Both values must be > 0 and should be written before start.
        serial_half_period_clks : in unsigned(31 downto 0);
        sample_period_clks      : in unsigned(31 downto 0);
        
        -- IP Code internal signals
        ip_busy  : out std_logic;
        
        -- Expernal Connections
        card_data : in  std_logic_vector(G_NUM_CARDS - 1 downto 0);
        busy      : in std_logic;
        meas_clk  : out std_logic;
        conv      : out std_logic;
        
        -- AXI Stream
        m_axis_tdata  : out std_logic_vector(G_AXIS_BITS - 1 downto 0);
        m_axis_tkeep  : out std_logic_vector(G_AXIS_BITS/8 - 1 downto 0);
        m_axis_tvalid : out std_logic;
        m_axis_tready : in  std_logic;
        m_axis_tlast  : out std_logic
    );
--  Port ( );
end Acquisition_Core;




---- #########################################
---- ############  ARCHITECTURE  #############
---- #########################################


architecture Behavioral of Acquisition_Core is

-- Default counter values. These are only used as safe reset values.
constant C_DEFAULT_SERIAL_HALF_PERIOD_CLKS : unsigned(31 downto 0) :=
    to_unsigned(G_CLK_HZ / (2 * G_SERIAL_CLK_HZ), 32);
constant C_DEFAULT_SAMPLE_PERIOD_CLKS : unsigned(31 downto 0) :=
    to_unsigned(G_CLK_HZ / G_SAMPLE_RATE_HZ, 32);

-- SIGNALS

-- State
type t_state is (
    ST_IDLE,
    ST_CONV,
    ST_ACQUIRE,
    ST_SEND,
    ST_WAIT_NEXT_SAMPLE
);
signal state : t_state := ST_IDLE;

-- Shift Registers to clock data in
type t_card_array is array (0 to G_NUM_CARDS - 1)
    of std_logic_vector(G_BITS_PER_CARD - 1 downto 0);
signal shift_reg : t_card_array := (others => (others => '0'));

-- Registers to mirror output signals
signal serial_clk_reg : std_logic := '0';
signal conversion_reg : std_logic := '0';

signal bit_cnt        : integer range 0 to G_BITS_PER_CARD := 0;
signal wait_conv_cnt : unsigned(31 downto 0) := (others => '0');
signal sample_cnt : integer range 0 to 100_000 - 1 := 0;
signal card_cnt   : integer range 0 to G_NUM_CARDS - 1 := 0;

signal serial_half_period_cnt : unsigned(31 downto 0) := (others => '0');
signal sample_period_cnt      : unsigned(31 downto 0) := (others => '0');

-- Configuration values are latched on start, so software can not change timing in the middle of an acquisition.
signal cfg_serial_half_period_clks : unsigned(31 downto 0) := C_DEFAULT_SERIAL_HALF_PERIOD_CLKS;
signal cfg_sample_period_clks      : unsigned(31 downto 0) := C_DEFAULT_SAMPLE_PERIOD_CLKS;


begin

-- Set outputs to mirror internal signals
ip_busy  <= '1' when state /= ST_IDLE else '0';
meas_clk <= serial_clk_reg;
conv     <= conversion_reg;

-- AXI Stream
m_axis_tdata  <= std_logic_vector(shift_reg(card_cnt));
m_axis_tkeep  <= (others => '1');
m_axis_tvalid <= '1' when state = ST_SEND else '0';
m_axis_tlast <= '1' when
        state = ST_SEND and
        sample_cnt = num_samples - 1 and
        card_cnt = G_NUM_CARDS - 1
    else '0';



  process(clk_sys)
  begin
    if rising_edge(clk_sys) then
    
    -- Reset
    if rst_n = '0' then

        state                   <= ST_IDLE;
        shift_reg               <= (others => (others => '0'));
        serial_clk_reg          <= '0';
        serial_half_period_cnt  <= (others => '0');
        bit_cnt                 <= 0;
        sample_cnt              <= 0;
        card_cnt                <= 0;
        sample_period_cnt       <= (others => '0');
        cfg_serial_half_period_clks <= C_DEFAULT_SERIAL_HALF_PERIOD_CLKS;
        cfg_sample_period_clks      <= C_DEFAULT_SAMPLE_PERIOD_CLKS;

      -- Normal Operation
      else
      
        -- State Machine
        case state is
        
            when ST_IDLE =>
    
                serial_clk_reg    <= '0';
                serial_half_period_cnt    <= (others => '0');
                bit_cnt           <= 0;
                sample_cnt <= 0;
                card_cnt          <= 0;
                conversion_reg    <= '0';
                sample_period_cnt <= (others => '0');
    
                if (start = '1') then
                  if (num_samples = 0 or serial_half_period_clks = 0 or sample_period_clks = 0) then
--                    error_reg <= '1';
                  else
--                    error_reg <= '0';
--                    shift_reg <= (others => (others => '0'));
                    cfg_serial_half_period_clks <= serial_half_period_clks;
                    cfg_sample_period_clks      <= sample_period_clks;
                    wait_conv_cnt <= (others => '0');
                    state     <= ST_CONV;
                  end if;
                end if;
                
            when ST_CONV =>
            
                -- Continue sample period count
                if sample_period_cnt < cfg_sample_period_clks - 1 then
                    sample_period_cnt <= sample_period_cnt + 1;
                end if;
            
                -- Conversion Delay
                -- Count to 500 at 100MHz -> 5us
                if (wait_conv_cnt < 500) then
                    conversion_reg    <= '1';
                    wait_conv_cnt <= wait_conv_cnt + 1;
                else
                    conversion_reg    <= '0';
                    shift_reg <= (others => (others => '0'));
                    bit_cnt        <= 0;
                    state             <= ST_ACQUIRE;
                end if;

            when ST_ACQUIRE =>

                -- Continue sample period count
                if sample_period_cnt < cfg_sample_period_clks - 1 then
                    sample_period_cnt <= sample_period_cnt + 1;
                end if;
                
                -- Toggle at half period
                if serial_half_period_cnt >= cfg_serial_half_period_clks - 1 then
                    serial_half_period_cnt <= (others => '0');
                    serial_clk_reg <= not serial_clk_reg;
                    
                    -- Falling Edge -> was 1 -> becomes 0
                    -- Clock in data line states
                    if serial_clk_reg = '1' then
                        for i in 0 to G_NUM_CARDS - 1 loop
                            -- Reg(59-0) = (Reg(58-0) << 1) & card_data(i) - Data is shifted in MSB first!
                            shift_reg(i) <= shift_reg(i)(G_BITS_PER_CARD - 2 downto 0) & card_data(i);
                        end loop;
                        
                        if bit_cnt = G_BITS_PER_CARD - 1 then
                          bit_cnt        <= 0;
                          serial_clk_reg <= '0';
                          serial_half_period_cnt <= (others => '0');
                          card_cnt       <= 0;
                          state          <= ST_SEND;
                        else
                          bit_cnt <= bit_cnt + 1;
                        end if;
                    end if;
                else
                    serial_half_period_cnt <= serial_half_period_cnt + 1;
                end if;
                
            when ST_SEND =>
            
                -- Continue sample period count
                if sample_period_cnt < cfg_sample_period_clks - 1 then
                    sample_period_cnt <= sample_period_cnt + 1;
                end if;
                
                
                -- AXI Stream send
                if m_axis_tready = '1' then
                
                    if card_cnt = G_NUM_CARDS - 1 then
                        card_cnt <= 0;
                    
                        if sample_cnt = num_samples - 1 then
                        -- Komplette Aufnahme fertig
                            sample_cnt <= 0;
                            state      <= ST_IDLE;
                        else
                            sample_cnt <= sample_cnt + 1;
                            state      <= ST_WAIT_NEXT_SAMPLE;
                        end if;
                        
                    else
                        card_cnt <= card_cnt + 1;
                    end if;
                
                end if;
                
                
--                if sample_cnt = num_samples - 1 then
--                  -- Komplette Aufnahme fertig
--                  sample_cnt <= 0;
----                  done_reg   <= '1';
--                  state      <= ST_IDLE;
--                else
--                  sample_cnt <= sample_cnt + 1;
--                  state      <= ST_WAIT_NEXT_SAMPLE;
--                end if;
      
      
          -------------------------------------------------------------------
          -- Waiting for next sample feriod to begin
          -------------------------------------------------------------------
          when ST_WAIT_NEXT_SAMPLE =>

            serial_clk_reg <= '0';

            if sample_period_cnt >= cfg_sample_period_clks - 1 then
              sample_period_cnt <= (others => '0');
              shift_reg         <= (others => (others => '0'));
              serial_half_period_cnt    <= (others => '0');
              bit_cnt           <= 0;
              wait_conv_cnt <= (others => '0');
              state             <= ST_CONV;
            else
              sample_period_cnt <= sample_period_cnt + 1;
            end if;
      
        end case;
      end if;
    end if;
  end process;


end Behavioral;
